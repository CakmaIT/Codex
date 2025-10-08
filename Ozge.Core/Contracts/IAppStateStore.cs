using Ozge.Core.State;

namespace Ozge.Core.Contracts;

public interface IAppStateStore
{
    AppState Current { get; }
    event EventHandler<AppState>? StateChanged;

    AppState Update(Func<AppStateBuilder, AppStateBuilder> reducer);
}
