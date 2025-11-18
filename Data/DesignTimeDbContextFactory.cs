using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RateMyTeacher.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        Env.TraversePath().Load();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var databaseSetting = configuration["DATABASE_PATH"] ?? "Data/ratemyteacher.db";
        var databasePath = Path.IsPathRooted(databaseSetting)
            ? databaseSetting
            : Path.Combine(basePath, databaseSetting);

        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite($"Data Source={databasePath}");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
