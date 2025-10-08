using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ozge.Core.Services;
using Ozge.Data.Context;
using Ozge.Data.Seed;

namespace Ozge.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOzgeData(this IServiceCollection services, string databasePath)
    {
        services.AddDbContextFactory<ApplicationDbContext>((provider, options) =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            options.UseSqlite(() => SqliteDataStore.CreateConnection(databasePath));
            options.UseLoggerFactory(loggerFactory);
            options.EnableSensitiveDataLogging(false);
        });

        services.AddSingleton<IDataStore, SqliteDataStore>();
        services.AddSingleton<DataSeeder>();
        return services;
    }
}
