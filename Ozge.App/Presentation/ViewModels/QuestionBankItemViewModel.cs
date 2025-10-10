using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ozge.App.Presentation.ViewModels;

public sealed class QuestionBankItemViewModel
{
    public QuestionBankItemViewModel(int order, string prompt, string correctAnswer, IEnumerable<string> options)
    {
        Order = order;
        Prompt = prompt;
        CorrectAnswer = correctAnswer;
        var optionList = options.ToList();
        Options = new ObservableCollection<string>(optionList);
        OptionCount = optionList.Count;
    }

    public int Order { get; }

    public string Prompt { get; }

    public string CorrectAnswer { get; }

    public ObservableCollection<string> Options { get; }

    public int OptionCount { get; }
}
