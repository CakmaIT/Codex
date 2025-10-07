using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ozge.Data.Context;

namespace Ozge.Data.Extensions;

public static class SqliteOptionsBuilderExtensions
{
    public static IServiceCollection AddOzgeSqlite(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OzgeDbContext>(options =>
        {
            var connectionString = BuildConnectionString();
            options.UseSqlite(connectionString);
            options.EnableSensitiveDataLogging(false);
        });

        return services;
    }

    public static string BuildConnectionString()
    {
        var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ozge2", "ozge2.db");
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true,
            Cache = SqliteCacheMode.Private
        };

        return builder.ToString();
    }

    public static void ApplyRecommendedPragmas(this SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA cache_size=-20000; PRAGMA busy_timeout=5000;";
        command.ExecuteNonQuery();
    }
}
