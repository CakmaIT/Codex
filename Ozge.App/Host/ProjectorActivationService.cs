using System.Windows;
using Ozge.App.ViewModels;
using Ozge.App.Views;
using Ozge.Core.Services;

namespace Ozge.App.Host;

public class ProjectorActivationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IScreenSelectionService _screenSelectionService;
    private ProjectorWindow? _window;

    public ProjectorActivationService(IServiceProvider serviceProvider, IScreenSelectionService screenSelectionService)
    {
        _serviceProvider = serviceProvider;
        _screenSelectionService = screenSelectionService;
    }

    public void TryShowProjectorWindow()
    {
        if (_window is not null)
        {
            return;
        }

        var window = _serviceProvider.GetRequiredService<ProjectorWindow>();
        window.DataContext = _serviceProvider.GetRequiredService<ProjectorViewModel>();
        window.WindowState = WindowState.Maximized;
        window.WindowStyle = WindowStyle.None;
        window.ResizeMode = ResizeMode.NoResize;

        var screens = System.Windows.Forms.Screen.AllScreens;
        var targetIndex = _screenSelectionService.ProjectorScreenIndex;
        if (targetIndex.HasValue && targetIndex.Value >= 0 && targetIndex.Value < screens.Length)
        {
            var workingArea = screens[targetIndex.Value].WorkingArea;
            window.Left = workingArea.Left;
            window.Top = workingArea.Top;
            window.Width = workingArea.Width;
            window.Height = workingArea.Height;
        }

        window.Show();
        _window = window;
    }
}
