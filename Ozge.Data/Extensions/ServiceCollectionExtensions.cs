using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ozge.Core.Contracts;
using Ozge.Data.Seeding;
using Ozge.Data.Services;
using Ozge.Data.Storage;

namespace Ozge.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOzgeData(this IServiceCollection services, string databasePath)
    {
        services.AddSingleton<SqlitePragmaConnectionInterceptor>();

        services.AddDbContextFactory<OzgeDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<SqlitePragmaConnectionInterceptor>();
            OzgeDbContextFactory.Configure(options, databasePath);
            options.AddInterceptors(interceptor);
        });

        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<OzgeDbContext>>().CreateDbContext());

        services.AddScoped<DataSeeder>();
        services.AddSingleton<DatabaseInitializer>();

        return services;
    }
}
