using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Ozge.Core.State;

namespace Ozge.App.Presentation.ViewModels;

public sealed class QuizQuestionViewModel
{
    public QuizQuestionViewModel(Guid questionId, int displayIndex, string prompt, IEnumerable<QuizOptionItemViewModel> options)
    {
        QuestionId = questionId;
        DisplayIndex = displayIndex;
        Prompt = prompt;
        Options = new ObservableCollection<QuizOptionItemViewModel>(options);
    }

    public Guid QuestionId { get; }

    public int DisplayIndex { get; }

    public string Prompt { get; }

    public string DisplayPrompt => $"{DisplayIndex}. {Prompt}";

    public ObservableCollection<QuizOptionItemViewModel> Options { get; }

    public static QuizQuestionViewModel FromState(QuizQuestionState state, int displayIndex)
    {
        return new QuizQuestionViewModel(
            state.QuestionId,
            displayIndex,
            state.Prompt,
            state.Options.Select((option, optionIndex) => new QuizOptionItemViewModel(
                option.Text,
                option.IsCorrect,
                optionIndex)));
    }

    public QuizQuestionState ToState() => new(
        QuestionId,
        Prompt,
        Options.Select(option => new QuizOptionState(option.Text, option.IsCorrect)).ToImmutableList());
}
