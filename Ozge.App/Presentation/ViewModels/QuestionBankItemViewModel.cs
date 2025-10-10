using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ozge.App.Presentation.ViewModels;

public sealed class QuestionBankItemViewModel
{
    public QuestionBankItemViewModel(string prompt, string correctAnswer, IEnumerable<string> options)
    {
        Prompt = prompt;
        CorrectAnswer = correctAnswer;
        Options = new ObservableCollection<string>(options);
    }

    public string Prompt { get; }

    public string CorrectAnswer { get; }

    public ObservableCollection<string> Options { get; }
}
