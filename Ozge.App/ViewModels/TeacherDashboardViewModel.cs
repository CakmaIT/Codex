using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ozge.App.Services;
using Ozge.App.State;
using Ozge.Core.Messaging;
using Ozge.Core.Models;
using Ozge.Core.Services;

namespace Ozge.App.ViewModels;

public partial class TeacherDashboardViewModel : ObservableRecipient
{
    private readonly StateStore _stateStore;
    private readonly IDataStore _dataStore;
    private readonly WindowCoordinator _coordinator;

    [ObservableProperty]
    private ObservableCollection<ClassTabViewModel> _classTabs = new();

    [ObservableProperty]
    private ObservableCollection<Unit> _units = new();

    [ObservableProperty]
    private ObservableCollection<Group> _groups = new();

    [ObservableProperty]
    private ObservableCollection<Question> _upcomingQuestions = new();

    [ObservableProperty]
    private Unit? _selectedUnit;

    [ObservableProperty]
    private string _activeMode = "HOME";

    [ObservableProperty]
    private bool _isAnswerRevealEnabled;

    public TeacherDashboardViewModel(StateStore stateStore, IDataStore dataStore, WindowCoordinator coordinator, IMessenger messenger)
        : base(messenger)
    {
        _stateStore = stateStore;
        _dataStore = dataStore;
        _coordinator = coordinator;
    }

    public async Task InitializeAsync()
    {
        await _stateStore.InitializeAsync();
        var state = _stateStore.State;
        ClassTabs = new ObservableCollection<ClassTabViewModel>(state.Classes.Values.Select(c => new ClassTabViewModel(c.Profile.Id, c.Profile.Name)));
        if (ClassTabs.Count > 0)
        {
            await LoadClassAsync(ClassTabs[0].Id);
        }
    }

    [RelayCommand]
    private async Task SwitchClass(Guid id)
    {
        await LoadClassAsync(id);
        _stateStore.SetActiveClass(id);
    }

    [RelayCommand]
    private async Task SetMode(string mode)
    {
        ActiveMode = mode;
        var unitId = SelectedUnit?.Id;
        if (unitId.HasValue)
        {
            var questions = await _dataStore.GetQuestionsAsync(unitId.Value);
            UpcomingQuestions = new ObservableCollection<Question>(questions.Take(10));
        }
        _stateStore.SetMode(mode, unitId);
        if (_coordinator.ProjectorWindow is null)
        {
            await LaunchProjector();
        }
    }

    [RelayCommand]
    private Task LaunchProjector()
    {
        _coordinator.OpenProjector();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task AwardPoints(Group group)
    {
        group.Score += 5;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task PenalizePoints(Group group)
    {
        group.Score -= 5;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenBehaviorLog(Group group)
    {
        Messenger.Send(new ToastMessage($"Behavior logged for {group.Name}"));
        return Task.CompletedTask;
    }

    partial void OnIsAnswerRevealEnabledChanged(bool value)
    {
        Messenger.Send(new RevealAnswerMessage(value));
    }

    private async Task LoadClassAsync(Guid classId)
    {
        var units = await _dataStore.GetUnitsAsync(classId);
        Units = new ObservableCollection<Unit>(units);
        SelectedUnit = Units.FirstOrDefault();
        var groups = await _dataStore.GetGroupsAsync(classId);
        Groups = new ObservableCollection<Group>(groups);
    }
}

public sealed record ClassTabViewModel(Guid Id, string Name);
