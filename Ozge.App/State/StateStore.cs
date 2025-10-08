using System.Collections.Immutable;
using CommunityToolkit.Mvvm.Messaging;
using Ozge.Core.Messaging;
using Ozge.Core.Models;
using Ozge.Core.Services;

namespace Ozge.App.State;

public sealed class StateStore
{
    private readonly IDataStore _dataStore;
    private readonly IMessenger _messenger;
    public AppState State { get; } = new();

    public StateStore(IDataStore dataStore, IMessenger messenger)
    {
        _dataStore = dataStore;
        _messenger = messenger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var classes = await _dataStore.GetClassesAsync(cancellationToken);
        var classStates = new List<ClassState>();
        foreach (var profile in classes)
        {
            var groups = await _dataStore.GetGroupsAsync(profile.Id, cancellationToken);
            var units = await _dataStore.GetUnitsAsync(profile.Id, cancellationToken);
            classStates.Add(new ClassState(profile, groups.ToImmutableList(), units.ToImmutableList()));
        }

        State.SetClasses(classStates);
        if (classStates.Count > 0)
        {
            State.ActiveClassId = classStates[0].Profile.Id;
        }
    }

    public void SetActiveClass(Guid classId)
    {
        State.ActiveClassId = classId;
        _messenger.Send(new ClassChangedMessage(classId));
    }

    public void SetMode(string mode, Guid? unitId = null)
    {
        State.ActiveMode = mode;
        State.ActiveUnitId = unitId;
        _messenger.Send(new ModeChangedMessage(mode, unitId));
    }
}
