namespace Ozge.App.Presentation.ViewModels;

public sealed class QuizOptionItemViewModel
{
    public QuizOptionItemViewModel(string text, bool isCorrect, int position)
    {
        Text = text;
        IsCorrect = isCorrect;
        Position = position;
    }

    public string Text { get; }

    public bool IsCorrect { get; }

    public int Position { get; }
}
