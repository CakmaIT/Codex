using System.Windows;
using Ozge.App.Views;

namespace Ozge.App.Services;

public sealed class WindowCoordinator
{
    private readonly IServiceProvider _serviceProvider;
    public TeacherDashboardWindow? TeacherWindow { get; private set; }
    public ProjectorWindow? ProjectorWindow { get; private set; }

    public WindowCoordinator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void RegisterTeacherWindow(TeacherDashboardWindow window)
    {
        TeacherWindow = window;
    }

    public void UnregisterTeacherWindow()
    {
        TeacherWindow = null;
        CloseProjector();
    }

    public void OpenProjector()
    {
        if (ProjectorWindow is not null)
        {
            ProjectorWindow.Activate();
            return;
        }

        ProjectorWindow = _serviceProvider.GetRequiredService<ProjectorWindow>();
        ProjectorWindow.Owner = TeacherWindow;
        ProjectorWindow.Show();
    }

    public void CloseProjector()
    {
        if (ProjectorWindow is not null)
        {
            ProjectorWindow.Close();
            ProjectorWindow = null;
        }
    }
}
