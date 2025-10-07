using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Services;

namespace Ozge.App.ViewModels;

public partial class TeacherDashboardViewModel : ObservableObject
{
    private readonly IAppStateStore _stateStore;
    private readonly IScreenSelectionService _screenSelectionService;

    [ObservableProperty]
    private ClassEntity? _activeClass;

    public ObservableCollection<ClassEntity> Classes { get; } = new();
    public ObservableCollection<GroupEntity> Groups { get; } = new();
    public ObservableCollection<UnitEntity> Units { get; } = new();

    [ObservableProperty]
    private string _selectedMode = "HOME";

    public TeacherDashboardViewModel(IAppStateStore stateStore, IScreenSelectionService screenSelectionService)
    {
        _stateStore = stateStore;
        _screenSelectionService = screenSelectionService;

        _stateStore.Changes.Subscribe(OnStateChanged);
    }

    private void OnStateChanged(AppStateSnapshot snapshot)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Classes.SyncWith(snapshot.Classes);
            var active = snapshot.ActiveClass;
            ActiveClass = active;
            if (active is not null)
            {
                Groups.SyncWith(snapshot.Groups.GetValueOrDefault(active.Id, Array.Empty<GroupEntity>()));
                Units.SyncWith(snapshot.Units.GetValueOrDefault(active.Id, Array.Empty<UnitEntity>()));
            }
        });
    }

    [RelayCommand]
    private async Task SetProjectorScreenAsync()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (screens.Length == 0)
        {
            return;
        }
        var nextIndex = ((_screenSelectionService.ProjectorScreenIndex ?? -1) + 1) % screens.Length;
        await _screenSelectionService.SetProjectorScreenIndexAsync(nextIndex);
    }
}

public static class ObservableCollectionExtensions
{
    public static void SyncWith<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
