using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ozge.Data.Seeding;

namespace Ozge.Data.Services;

public sealed class DatabaseInitializer
{
    private readonly IDbContextFactory<OzgeDbContext> _contextFactory;
    private readonly DataSeeder _seeder;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IDbContextFactory<OzgeDbContext> contextFactory,
        DataSeeder seeder,
        ILogger<DatabaseInitializer> logger)
    {
        _contextFactory = contextFactory;
        _seeder = seeder;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MigrateAsync failed, falling back to EnsureCreated.");
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        await _seeder.SeedAsync(cancellationToken);
    }
}
