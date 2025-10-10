using System;
using CommunityToolkit.Mvvm.Messaging;
using Ozge.App.Presentation.Messaging;
using Ozge.Core.Contracts;
using Ozge.Core.State;

namespace Ozge.App.Infrastructure;

public sealed class AppStateStore : IAppStateStore
{
    private readonly object _gate = new();
    private readonly IMessenger _messenger;
    private AppState _state = AppState.Empty;

    public AppStateStore(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public AppState Current
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    public event EventHandler<AppState>? StateChanged;

    public AppState Update(Func<AppStateBuilder, AppStateBuilder> reducer)
    {
        AppState snapshot;
        AppState previous;

        lock (_gate)
        {
            previous = _state;
            var builder = new AppStateBuilder(_state);
            snapshot = reducer(builder).Build();
            if (snapshot == _state)
            {
                return _state;
            }

            _state = snapshot;
        }

        StateChanged?.Invoke(this, snapshot);
        _messenger.Send(new AppStateChangedMessage(snapshot, previous));
        return snapshot;
    }
}
