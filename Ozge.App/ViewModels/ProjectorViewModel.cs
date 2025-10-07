using CommunityToolkit.Mvvm.ComponentModel;
using Ozge.Core.Services;

namespace Ozge.App.ViewModels;

public partial class ProjectorViewModel : ObservableObject
{
    private readonly IAppStateStore _stateStore;

    [ObservableProperty]
    private string _activeMode = "HOME";

    [ObservableProperty]
    private bool _isAnswerRevealed;

    public ProjectorViewModel(IAppStateStore stateStore)
    {
        _stateStore = stateStore;
        _stateStore.Changes.Subscribe(_ => { });
    }
}
