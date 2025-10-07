using System.Windows;
using Ozge.App.Services;
using Ozge.App.ViewModels;

namespace Ozge.App.Views;

public partial class TeacherDashboardWindow : Window
{
    private readonly TeacherDashboardViewModel _viewModel;
    private readonly WindowCoordinator _coordinator;

    public TeacherDashboardWindow(TeacherDashboardViewModel viewModel, WindowCoordinator coordinator)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _coordinator = coordinator;
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
        _coordinator.RegisterTeacherWindow(this);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _coordinator.UnregisterTeacherWindow();
    }
}
