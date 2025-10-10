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
    private string modeTitle = "EGLENCE";

    [ObservableProperty]
    private string prompt = "Ogretmen bekleniyor";

    [ObservableProperty]
    private string feedback = "Hazir misiniz?";

    [ObservableProperty]
    private string quizPrompt = string.Empty;

    [ObservableProperty]
    private string quizProgress = string.Empty;

    [ObservableProperty]
    private string quizProgressDetail = "0 / 0";

    [ObservableProperty]
    private double quizProgressValue;

    [ObservableProperty]
    private bool showAnswers;

    [ObservableProperty]
    private bool isQuizVisible;

    [ObservableProperty]
    private bool showCelebration;

    [ObservableProperty]
    private string? celebrationSoundPath;

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
        ModeTitle = GetModeTitle(state.ActiveMode);

        var activeClass = state.Classes.FirstOrDefault(x => x.Id == state.ActiveClassId);
        if (activeClass is not null)
        {
            Leaderboard.ReplaceWith(activeClass.Groups
                .OrderByDescending(x => x.Score)
                .Select(GroupScoreItem.FromState));
        }
        else
        {
            Leaderboard.Clear();
        }

        ShowAnswers = state.IsAnswerRevealEnabled;
        Feedback = state.IsAnswerRevealEnabled
            ? "Dogru cevaplar acik! Aferin!"
            : state.ActiveMode == LessonMode.Quiz
                ? "Cevabini secmeden once dusun."
                : "Enerjini yuksek tut!";

        Prompt = state.ActiveMode switch
        {
            LessonMode.Home => "Hazir olun! Birazdan ogretmeniniz baslatacak.",
            LessonMode.Quiz => "Odaklanin, yeni soru yolda.",
            LessonMode.Puzzle => "Harf parcalarini birlestir ve kelimeyi bul!",
            LessonMode.Speak => "Sesi ac ve yuksek sesle konus!",
            LessonMode.Story => "Hikayeyi dikkatle dinleyin.",
            LessonMode.Draw => "Cizim kalemlerinizi hazirlayin!",
            LessonMode.Bonus => "Bonus turu! En hizli olan kazanir.",
            LessonMode.Result => "Skorlar aciklaniyor! Alkislara hazir olun.",
            _ => "Hazir miyiz?"
        };

        CelebrationSoundPath = state.CelebrationSoundPath;
        UpdateQuizPresentation(state);
    }

    private void UpdateQuizPresentation(AppState state)
    {
        if (state.ActiveMode == LessonMode.Quiz &&
            state.Quiz.TotalQuestions > 0 &&
            state.Quiz.CurrentQuestion is not null)
        {
            var question = state.Quiz.CurrentQuestion;
            QuizPrompt = question.Prompt;
            var questionNumber = Math.Max(1, state.Quiz.CurrentQuestionIndex + 1);
            var totalQuestions = Math.Max(1, state.Quiz.TotalQuestions);
            QuizProgress = $"Soru {questionNumber}";
            QuizProgressDetail = $"{questionNumber} / {totalQuestions}";
            QuizProgressValue = Math.Clamp((double)questionNumber / totalQuestions, 0, 1);

            QuizOptions.ReplaceWith(question.Options
                .Select((option, index) =>
                    new ProjectorQuizOptionViewModel(
                        option.Text,
                        state.IsAnswerRevealEnabled && option.IsCorrect,
                        index)));

            IsQuizVisible = true;
            ShowCelebration = false;
        }
        else
        {
            QuizPrompt = string.Empty;
            QuizProgress = string.Empty;
            QuizProgressDetail = "0 / 0";
            QuizProgressValue = 0;
            QuizOptions.Clear();
            IsQuizVisible = false;
            ShowCelebration = state.ActiveMode == LessonMode.Result;
        }
    }

    private static string GetModeTitle(LessonMode mode) => mode switch
    {
        LessonMode.Home => "BASLANGIC",
        LessonMode.Quiz => "QUIZ ZAMANI",
        LessonMode.Puzzle => "BULMACA",
        LessonMode.Speak => "KONUSMA",
        LessonMode.Story => "HIKAYE",
        LessonMode.Draw => "CIZ & YAZ",
        LessonMode.Bonus => "BONUS",
        LessonMode.Result => "SONUCLAR",
        _ => "EGLENCE"
    };
}
