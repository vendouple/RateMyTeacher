using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RateMyTeacher.Data;
using RateMyTeacher.Data.Seed;
using RateMyTeacher.Models;
using RateMyTeacher.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

Env.TraversePath().Load();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder
    .Services
    .AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IBonusService, BonusService>();
builder.Services.AddScoped<IAIUsageService, AIUsageService>();

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("id-ID"),
    new CultureInfo("zh-CN")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var databaseSetting = builder.Configuration["DATABASE_PATH"] ?? "Data/ratemyteacher.db";
var databasePath = Path.IsPathRooted(databaseSetting)
    ? databaseSetting
    : Path.Combine(builder.Environment.ContentRootPath, databaseSetting);

var dataDirectory = Path.GetDirectoryName(databasePath);
if (!string.IsNullOrEmpty(dataDirectory) && !Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

var connectionString = $"Data Source={databasePath}";

builder
    .Services
    .AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));

builder
    .Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope
        .ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Database");
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        dbContext.Database.Migrate();
        SeedData.Initialize(dbContext);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
