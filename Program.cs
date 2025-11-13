using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var databaseSetting = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? "Data/ratemyteacher.db";
var databasePath = Path.IsPathRooted(databaseSetting)
    ? databaseSetting
    : Path.Combine(builder.Environment.ContentRootPath, databaseSetting);

var dataDirectory = Path.GetDirectoryName(databasePath);
if (!string.IsNullOrEmpty(dataDirectory) && !Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

var connectionString = $"Data Source={databasePath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services
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
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    RateMyTeacher.Data.Seed.SeedData.Initialize(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
