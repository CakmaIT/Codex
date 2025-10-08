namespace Ozge.App.Presentation.ViewModels;

public sealed class ProjectorQuizOptionViewModel
{
    public ProjectorQuizOptionViewModel(string text, bool isHighlighted)
    {
        Text = text;
        IsHighlighted = isHighlighted;
    }

    public string Text { get; }
    public bool IsHighlighted { get; }
}
