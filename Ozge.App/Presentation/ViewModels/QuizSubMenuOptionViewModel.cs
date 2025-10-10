namespace Ozge.App.Presentation.ViewModels;

public enum QuizSubMenuKey
{
    Control,
    QuestionBank
}

public sealed class QuizSubMenuOptionViewModel
{
    private QuizSubMenuOptionViewModel(QuizSubMenuKey key, string title)
    {
        Key = key;
        Title = title;
    }

    public QuizSubMenuKey Key { get; }

    public string Title { get; }

    public static QuizSubMenuOptionViewModel Create(QuizSubMenuKey key, string title) =>
        new(key, title);

    public override string ToString() => Title;
}
