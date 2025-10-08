using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ozge.Core;
using Ozge.Core.Contracts;
using Ozge.Core.Domain.Enums;
using Ozge.Core.State;
using Ozge.Data;
using Ozge.Data.Services;

namespace Ozge.App.Infrastructure;

public sealed class AppInitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IAssetProvisioner _assetProvisioner;
    private readonly ILogger<AppInitializationHostedService> _logger;
    private bool _initialized;

    public AppInitializationHostedService(
        IServiceProvider serviceProvider,
        IFileSystem fileSystem,
        IAssetProvisioner assetProvisioner,
        ILogger<AppInitializationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _fileSystem = fileSystem;
        _assetProvisioner = assetProvisioner;
        _logger = logger;
    }

    public async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await InitializeAsync(CancellationToken.None);
    }

    public Task StartAsync(CancellationToken cancellationToken) => InitializeAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            CreateAppDirectories();
            await _assetProvisioner.EnsureAssetsAsync(cancellationToken);
            _logger.LogInformation("Assets ensured under {Root}", AppConstants.GetLocalAppDataRoot());
            await RunDatabaseInitializationAsync(cancellationToken);
            await HydrateStateAsync(cancellationToken);
            _initialized = true;
            _logger.LogInformation("Application initialisation completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application initialization failed");
            throw;
        }
    }

    private async Task RunDatabaseInitializationAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        _logger.LogInformation("Running database migrations and seed...");
        await initializer.InitializeAsync(cancellationToken);
    }

    private async Task HydrateStateAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OzgeDbContext>();
        var stateStore = scope.ServiceProvider.GetRequiredService<IAppStateStore>();

        var classes = await db.Classes
            .Include(c => c.Groups)
            .Include(c => c.Units).ThenInclude(u => u.Words)
            .Include(c => c.Units).ThenInclude(u => u.Questions)
            .Include(c => c.Students)
            .Include(c => c.LessonLogs)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var classStates = classes.Select(MapClass).ToImmutableList();
        var activeClassId = classStates.FirstOrDefault()?.Id ?? Guid.Empty;

        stateStore.Update(builder => builder
            .SetClasses(classStates)
            .WithActiveClass(activeClassId));
        _logger.LogInformation("Hydrated application state with {ClassCount} classes.", classStates.Count);
    }

    private void CreateAppDirectories()
    {
        var root = AppConstants.GetLocalAppDataRoot();
        _fileSystem.CreateDirectory(root);
        _fileSystem.CreateDirectory(Path.Combine(root, AppConstants.SnapshotsFolderName));
        _fileSystem.CreateDirectory(Path.Combine(root, AppConstants.BackupsFolderName));
        _fileSystem.CreateDirectory(Path.Combine(root, AppConstants.LogsFolderName));
    }

    private static ClassState MapClass(Ozge.Core.Domain.Entities.ClassEntity entity)
    {
        var groups = entity.Groups
            .Select(group => new GroupState(
                group.Id,
                group.Name,
                group.Avatar,
                group.Score,
                0,
                false,
                DateTimeOffset.UtcNow))
            .ToImmutableList();

        var units = entity.Units
            .Select(unit => new UnitSummary(
                unit.Id,
                unit.Name,
                unit.Topic,
                unit.Difficulty,
                unit.Words.Count,
                unit.Questions.Count,
                false))
            .ToImmutableList();

        var students = entity.Students
            .Select(student => new StudentState(
                student.Id,
                student.Name,
                student.Seat,
                true,
                student.IsActive))
            .ToImmutableList();

        var lessonLogs = entity.LessonLogs
            .Select(log => new LessonLogSnapshot(
                log.SessionId,
                LessonMode.Quiz,
                null,
                log.Timestamp,
                log.Timestamp,
                "Demo lesson"))
            .ToImmutableList();

        return new ClassState(
            entity.Id,
            entity.Name,
            groups,
            units,
            students,
            ImmutableList<ScoreSnapshot>.Empty,
            ImmutableList<BehaviorSnapshot>.Empty,
            lessonLogs);
    }
}

internal static class AppStateBuilderExtensions
{
    public static AppStateBuilder WithActiveClass(this AppStateBuilder builder, Guid classId)
    {
        builder.ActiveClassId = classId;
        builder.ActiveMode = LessonMode.Home;
        return builder;
    }
}
