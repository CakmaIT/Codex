namespace Ozge.App.Presentation.ViewModels;

public sealed class QuizOptionItemViewModel
{
    public QuizOptionItemViewModel(string text, bool isCorrect)
    {
        Text = text;
        IsCorrect = isCorrect;
    }

    public string Text { get; }
    public bool IsCorrect { get; }
}
