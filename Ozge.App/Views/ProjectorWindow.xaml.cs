using System.Windows;
using Ozge.App.ViewModels;

namespace Ozge.App.Views;

public partial class ProjectorWindow : Window
{
    private readonly ProjectorViewModel _viewModel;

    public ProjectorWindow(ProjectorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}
