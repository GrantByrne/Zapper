using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Zapper.Data;

public static class DatabaseExtensions
{
    /// <summary>
    /// Configures the ZapperContext to use SQLite database in ~/.zapper/zapper.db
    /// </summary>
    public static IServiceCollection AddZapperDatabase(this IServiceCollection services)
    {
        var databasePath = GetDatabasePath();

        // Ensure the ~/.zapper directory exists
        var zapperDirectory = Path.GetDirectoryName(databasePath)!;
        Directory.CreateDirectory(zapperDirectory);

        services.AddDbContext<ZapperContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        return services;
    }

    /// <summary>
    /// Ensures the database is created and all migrations are applied.
    /// Creates timestamped backups before applying migrations to existing databases.
    /// </summary>
    public static async Task EnsureDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ZapperContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ZapperContext>>();

        await EnsureDatabaseAsync(context, logger);
    }

    /// <summary>
    /// Ensures the database is created and all migrations are applied.
    /// Creates timestamped backups before applying migrations to existing databases.
    /// </summary>
    public static async Task EnsureDatabaseAsync(ZapperContext context, ILogger logger)
    {
        try
        {
            var databasePath = GetDatabasePath();
            var zapperDirectory = Path.GetDirectoryName(databasePath)!;

            // Check if database exists and has pending migrations
            var databaseExists = File.Exists(databasePath);
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

            if (databaseExists && pendingMigrations.Any())
            {
                // Create backup before applying migrations
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(zapperDirectory, $"zapper_backup_{timestamp}.db");

                logger.LogInformation("Creating database backup before migration: {BackupPath}", backupPath);
                File.Copy(databasePath, backupPath);
                logger.LogInformation("Database backup created successfully");
            }

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending database migrations...", pendingMigrations.Count);
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else if (databaseExists)
            {
                logger.LogInformation("Database is up to date, no migrations needed");
            }
            else
            {
                logger.LogInformation("Creating new database...");
                await context.Database.MigrateAsync();
                logger.LogInformation("New database created successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations");
            throw;
        }
    }

    /// <summary>
    /// Gets the standard database path: ~/.zapper/zapper.db
    /// </summary>
    public static string GetDatabasePath()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var zapperDirectory = Path.Combine(homeDirectory, ".zapper");
        return Path.Combine(zapperDirectory, "zapper.db");
    }

    /// <summary>
    /// Gets the directory where Zapper stores its data: ~/.zapper
    /// </summary>
    public static string GetZapperDirectory()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDirectory, ".zapper");
    }
}