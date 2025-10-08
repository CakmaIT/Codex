using System.Windows;
using Ozge.App.Presentation.ViewModels;

namespace Ozge.App.Presentation.Windows;

public partial class ProjectorWindow : Window
{
    private readonly ProjectorViewModel _viewModel;

    public ProjectorWindow(ProjectorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public void SetFreezeOverlay(bool isFrozen)
    {
        FreezeOverlay.Visibility = isFrozen ? Visibility.Visible : Visibility.Collapsed;
    }

    public void SetAnswerReveal(bool reveal)
    {
        AnswerOverlay.Visibility = reveal ? Visibility.Visible : Visibility.Collapsed;
    }
}
