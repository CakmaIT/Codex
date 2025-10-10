using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ozge.App.Presentation.ViewModels;

public sealed partial class ProjectorQuizOptionViewModel : ObservableObject
{
    public ProjectorQuizOptionViewModel(string text, bool isCorrect, int position)
    {
        Text = text;
        IsCorrect = isCorrect;
        Position = position;
    }

    public string Text { get; }

    public bool IsCorrect { get; }

    public int Position { get; }

    public string Label => ((char)('A' + Position)).ToString(CultureInfo.InvariantCulture);

    [ObservableProperty]
    private bool isHighlighted;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isIncorrectSelection;
}
