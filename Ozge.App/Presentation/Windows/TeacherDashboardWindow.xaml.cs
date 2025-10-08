using System.Windows;
using Ozge.App.Presentation.ViewModels;

namespace Ozge.App.Presentation.Windows;

public partial class TeacherDashboardWindow : Window
{
    public TeacherDashboardWindow(TeacherDashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
