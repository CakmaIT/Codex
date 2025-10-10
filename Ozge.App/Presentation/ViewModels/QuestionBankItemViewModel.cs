using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ozge.App.Presentation.ViewModels;

public sealed class QuestionBankItemViewModel
{
    public QuestionBankItemViewModel(int displayIndex, string prompt, string correctAnswer, IEnumerable<string> options)
    {
        DisplayIndex = displayIndex;
        Prompt = prompt;
        CorrectAnswer = correctAnswer;
        Options = new ObservableCollection<string>(options);
    }

    public int DisplayIndex { get; }

    public string Prompt { get; }

    public string CorrectAnswer { get; }

    public ObservableCollection<string> Options { get; }

    public int OptionCount => Options.Count;

    public string DisplayTitle => $"Soru {DisplayIndex}";
}
