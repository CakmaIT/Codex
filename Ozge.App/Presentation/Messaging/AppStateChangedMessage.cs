using CommunityToolkit.Mvvm.Messaging.Messages;
using Ozge.Core.State;

namespace Ozge.App.Presentation.Messaging;

public sealed class AppStateChangedMessage : ValueChangedMessage<AppState>
{
    public AppState Previous { get; }

    public AppStateChangedMessage(AppState value, AppState previous)
        : base(value)
    {
        Previous = previous;
    }
}
