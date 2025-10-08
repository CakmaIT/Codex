using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ozge.App.Presentation.Messaging;
using Ozge.Core.Domain.Enums;
using Ozge.Core.State;

namespace Ozge.App.Presentation.ViewModels;

public sealed partial class ProjectorViewModel : ViewModelBase, IRecipient<AppStateChangedMessage>
{
    public ObservableCollection<GroupScoreItem> Leaderboard { get; } = new();
    public ObservableCollection<ProjectorQuizOptionViewModel> QuizOptions { get; } = new();

    [ObservableProperty]
    private string modeTitle = "HOME";

    [ObservableProperty]
    private string prompt = "Waiting for teacher";

    [ObservableProperty]
    private string feedback = string.Empty;

    [ObservableProperty]
    private string quizPrompt = string.Empty;

    [ObservableProperty]
    private string quizProgress = string.Empty;

    [ObservableProperty]
    private bool showAnswers;

    [ObservableProperty]
    private bool isQuizVisible;

    public ProjectorViewModel(IMessenger messenger)
        : base(messenger)
    {
    }

    public void Receive(AppStateChangedMessage message)
    {
        UpdateFromState(message.Value);
    }

    private void UpdateFromState(AppState state)
    {
        ModeTitle = state.ActiveMode.ToString().ToUpperInvariant();

        var activeClass = state.Classes.FirstOrDefault(x => x.Id == state.ActiveClassId);
        if (activeClass is not null)
        {
            Leaderboard.ReplaceWith(activeClass.Groups.OrderByDescending(x => x.Score).Select(GroupScoreItem.FromState));
        }
        else
        {
            Leaderboard.Clear();
        }

        ShowAnswers = state.IsAnswerRevealEnabled;
        Feedback = state.IsAnswerRevealEnabled
            ? "Answers visible"
            : state.ActiveMode == LessonMode.Quiz
                ? "Think carefully before answering."
                : "Keep guessing!";

        Prompt = state.ActiveMode switch
        {
            LessonMode.Home => "Welcome! Waiting for teacher to begin.",
            LessonMode.Quiz => "Get ready for the next question.",
            LessonMode.Puzzle => "Arrange the letters to solve the puzzle.",
            LessonMode.Speak => "Speak clearly into the microphone.",
            LessonMode.Story => "Listen carefully to the story.",
            LessonMode.Draw => "Prepare to draw and spell!",
            LessonMode.Bonus => "Bonus blitz! Stay focused.",
            LessonMode.Result => "Here are the results!",
            _ => string.Empty
        };

        UpdateQuizPresentation(state);
    }

    private void UpdateQuizPresentation(AppState state)
    {
        if (state.ActiveMode == LessonMode.Quiz && state.Quiz.CurrentQuestion is not null)
        {
            var question = state.Quiz.CurrentQuestion;
            QuizPrompt = question.Prompt;
            QuizProgress = $"Question {Math.Max(1, state.Quiz.CurrentQuestionIndex + 1)} / {Math.Max(1, state.Quiz.TotalQuestions)}";

            QuizOptions.ReplaceWith(question.Options.Select(option =>
                new ProjectorQuizOptionViewModel(
                    option.Text,
                    state.IsAnswerRevealEnabled && option.IsCorrect)));

            IsQuizVisible = true;
        }
        else
        {
            QuizPrompt = string.Empty;
            QuizProgress = string.Empty;
            QuizOptions.Clear();
            IsQuizVisible = false;
        }
    }
}
