using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ozge.Core;

namespace Ozge.Data.Storage;

public sealed class OzgeDbContextFactory : IDesignTimeDbContextFactory<OzgeDbContext>
{
    public OzgeDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<OzgeDbContext>();
        Configure(builder);
        return new OzgeDbContext(builder.Options);
    }

    public static void Configure(DbContextOptionsBuilder builder, string? databasePath = null)
    {
        var dbPath = databasePath ?? Path.Combine(AppConstants.GetLocalAppDataRoot(), AppConstants.DatabaseFileName);
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connectionString = $"Data Source={dbPath}";
        builder.UseSqlite(connectionString);
        builder.AddInterceptors(new SqlitePragmaConnectionInterceptor());
    }
}
