using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ozge.App.Infrastructure;
using Ozge.App.Presentation.Messaging;
using Ozge.Core.Domain.Enums;
using Ozge.Core.State;

namespace Ozge.App.Presentation.ViewModels;

public sealed partial class ProjectorViewModel : ViewModelBase, IRecipient<AppStateChangedMessage>
{
    private readonly ISoundEffectPlayer _soundEffectPlayer;
    private AppState _latestState = AppState.Empty;
    private CancellationTokenSource? _feedbackCancellation;

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
    private bool showCorrectSelectionFeedback;

    [ObservableProperty]
    private bool showIncorrectSelectionFeedback;

    public ProjectorViewModel(
        IMessenger messenger,
        ISoundEffectPlayer soundEffectPlayer)
        : base(messenger)
    {
        _soundEffectPlayer = soundEffectPlayer;
    }

    public void Receive(AppStateChangedMessage message)
    {
        UpdateFromState(message.Value);
    }

    private void UpdateFromState(AppState state)
    {
        _latestState = state;
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

        UpdateQuizPresentation(state);
    }

    private void UpdateQuizPresentation(AppState state)
    {
        CancelFeedback();

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

            var options = question.Options
                .Select((option, index) => new ProjectorQuizOptionViewModel(
                    option.Text,
                    option.IsCorrect,
                    index))
                .ToList();

            QuizOptions.ReplaceWith(options);
            ResetOptionStates();

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
            ShowCelebration = state.ActiveMode == LessonMode.Result && state.Quiz.TotalQuestions > 0;
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

    [RelayCommand]
    private async Task SelectOptionAsync(ProjectorQuizOptionViewModel? option)
    {
        if (option is null ||
            _latestState.ActiveMode != LessonMode.Quiz ||
            _latestState.Quiz.CurrentQuestion is null)
        {
            return;
        }

        var isCorrect = option.IsCorrect;

        foreach (var quizOption in QuizOptions)
        {
            var isSelected = ReferenceEquals(quizOption, option);
            quizOption.IsSelected = isSelected;
            quizOption.IsIncorrectSelection = isSelected && !isCorrect;
            quizOption.IsHighlighted = (ShowAnswers && quizOption.IsCorrect) || (quizOption.IsCorrect && isSelected);
        }

        Feedback = isCorrect
            ? "Harika secim! Boyle devam."
            : "Tekrar denemekten cekinme, dogrusunu bulacaksin.";

        ShowCorrectSelectionFeedback = isCorrect;
        ShowIncorrectSelectionFeedback = !isCorrect;

        CancelFeedback();
        _feedbackCancellation = new CancellationTokenSource();
        var token = _feedbackCancellation.Token;

        if (isCorrect)
        {
            await _soundEffectPlayer.PlayCorrectAsync();
        }
        else
        {
            await _soundEffectPlayer.PlayIncorrectAsync();
        }

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2.5), token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        ShowCorrectSelectionFeedback = false;
        ShowIncorrectSelectionFeedback = false;

        foreach (var quizOption in QuizOptions)
        {
            if (!quizOption.IsCorrect)
            {
                quizOption.IsSelected = false;
                quizOption.IsIncorrectSelection = false;
            }

            quizOption.IsHighlighted = ShowAnswers && quizOption.IsCorrect;
        }
    }

    public void ResetAfterHide()
    {
        CancelFeedback();
        ShowCorrectSelectionFeedback = false;
        ShowIncorrectSelectionFeedback = false;
        ResetOptionStates();
    }

    partial void OnShowAnswersChanged(bool value)
    {
        foreach (var option in QuizOptions)
        {
            option.IsHighlighted = (value && option.IsCorrect) || (option.IsCorrect && option.IsSelected);
        }
    }

    private void ResetOptionStates()
    {
        foreach (var option in QuizOptions)
        {
            option.IsSelected = false;
            option.IsIncorrectSelection = false;
            option.IsHighlighted = ShowAnswers && option.IsCorrect;
        }
    }

    private void CancelFeedback()
    {
        if (_feedbackCancellation is null)
        {
            return;
        }

        _feedbackCancellation.Cancel();
        _feedbackCancellation.Dispose();
        _feedbackCancellation = null;
    }

    partial void OnShowCelebrationChanged(bool value)
    {
        if (value)
        {
            _ = _soundEffectPlayer.PlayCelebrationAsync();
        }
    }
}

