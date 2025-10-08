using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ozge.Core.Models;
using Ozge.Core.Services;
using Ozge.Data.Context;

namespace Ozge.Data;

public class SqliteDataStore : IDataStore
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<SqliteDataStore> _logger;

    public SqliteDataStore(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<SqliteDataStore> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClassProfile>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Classes.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<ClassProfile> GetClassAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Classes.AsNoTracking().FirstAsync(x => x.Id == classId, cancellationToken);
        return entity;
    }

    public async Task SaveClassAsync(ClassProfile profile, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.Classes.FirstOrDefaultAsync(x => x.Id == profile.Id, cancellationToken);
        if (existing is null)
        {
            context.Classes.Add(profile);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(profile);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Group>> GetGroupsAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Groups.AsNoTracking().Where(x => x.ClassId == classId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Unit>> GetUnitsAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Units.AsNoTracking().Where(x => x.ClassId == classId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Word>> GetWordsAsync(Guid unitId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Words.AsNoTracking().Where(x => x.UnitId == unitId).OrderBy(x => x.Text).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetQuestionsAsync(Guid unitId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Questions.AsNoTracking().Where(x => x.UnitId == unitId).OrderBy(x => x.Difficulty).ToListAsync(cancellationToken);
    }

    public async Task AppendScoreEventAsync(ScoreEvent scoreEvent, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.ScoreEvents.Add(scoreEvent);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AppendBehaviorEventAsync(BehaviorEvent behaviorEvent, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.BehaviorEvents.Add(behaviorEvent);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AppendLessonLogAsync(LessonLog log, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.LessonLogs.Add(log);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static SqliteConnection CreateConnection(string databasePath)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();
        command.CommandText = "PRAGMA synchronous=NORMAL;";
        command.ExecuteNonQuery();
        command.CommandText = "PRAGMA cache_size=-20000;";
        command.ExecuteNonQuery();
        command.CommandText = "PRAGMA busy_timeout=5000;";
        command.ExecuteNonQuery();
        return connection;
    }
}
