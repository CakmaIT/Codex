using System.Globalization;

namespace Ozge.App.Presentation.ViewModels;

public sealed class ProjectorQuizOptionViewModel
{
    public ProjectorQuizOptionViewModel(string text, bool isHighlighted, int position)
    {
        Text = text;
        IsHighlighted = isHighlighted;
        Position = position;
    }

    public string Text { get; }

    public bool IsHighlighted { get; }

    public int Position { get; }

    public string Label => ((char)('A' + Position)).ToString(CultureInfo.InvariantCulture);
}
