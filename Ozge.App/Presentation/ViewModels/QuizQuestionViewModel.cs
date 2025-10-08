using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Ozge.Core.State;

namespace Ozge.App.Presentation.ViewModels;

public sealed class QuizQuestionViewModel
{
    public QuizQuestionViewModel(Guid questionId, string prompt, IEnumerable<QuizOptionItemViewModel> options)
    {
        QuestionId = questionId;
        Prompt = prompt;
        Options = new ObservableCollection<QuizOptionItemViewModel>(options);
    }

    public Guid QuestionId { get; }

    public string Prompt { get; }

    public ObservableCollection<QuizOptionItemViewModel> Options { get; }

    public static QuizQuestionViewModel FromState(QuizQuestionState state)
    {
        return new QuizQuestionViewModel(
            state.QuestionId,
            state.Prompt,
            state.Options.Select(option => new QuizOptionItemViewModel(option.Text, option.IsCorrect)));
    }

    public QuizQuestionState ToState() => new(
        QuestionId,
        Prompt,
        Options.Select(option => new QuizOptionState(option.Text, option.IsCorrect)).ToImmutableList());
}
