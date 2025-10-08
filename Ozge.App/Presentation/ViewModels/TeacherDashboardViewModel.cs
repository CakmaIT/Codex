using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ozge.App.Presentation.Messaging;
using Ozge.Core.Contracts;
using Ozge.Core.Domain.Enums;
using Ozge.Core.State;

namespace Ozge.App.Presentation.ViewModels;

public sealed partial class TeacherDashboardViewModel : ViewModelBase, IRecipient<AppStateChangedMessage>
{
    private readonly IAppStateStore _stateStore;
    private readonly IProjectorWindowManager _projectorWindowManager;
    private readonly IQuizSessionService _quizSessionService;

    public ObservableCollection<ClassItemViewModel> Classes { get; } = new();
    public ObservableCollection<GroupScoreItem> Leaderboard { get; } = new();
    public ObservableCollection<QuizQuestionViewModel> QuizQuestions { get; } = new();

    [ObservableProperty]
    private ClassItemViewModel? selectedClass;

    [ObservableProperty]
    private UnitSummary? selectedUnit;

    [ObservableProperty]
    private QuizQuestionViewModel? currentQuestion;

    [ObservableProperty]
    private bool isAnswerRevealEnabled;

    [ObservableProperty]
    private bool isProjectorFrozen;

    [ObservableProperty]
    private bool isQuizActive;

    [ObservableProperty]
    private bool isQuizLoading;

    [ObservableProperty]
    private bool canStartQuiz;

    [ObservableProperty]
    private bool canAdvanceQuestion;

    [ObservableProperty]
    private bool canEndQuiz;

    [ObservableProperty]
    private int currentQuestionNumber;

    [ObservableProperty]
    private int totalQuestions;

    [ObservableProperty]
    private string quizStatus = "Quiz not started";

    [ObservableProperty]
    private string answerRevealButtonText = "Reveal Answers";

    [ObservableProperty]
    private string freezeButtonText = "Freeze Projector";

    [ObservableProperty]
    private string projectorStatus = "Closed";

    [ObservableProperty]
    private string answerRevealStatus = "Hidden";

    [ObservableProperty]
    private string projectorFullScreenButtonText = "Go Full Screen";

    public TeacherDashboardViewModel(
        IAppStateStore stateStore,
        IProjectorWindowManager projectorWindowManager,
        IQuizSessionService quizSessionService,
        IMessenger messenger)
        : base(messenger)
    {
        _stateStore = stateStore;
        _projectorWindowManager = projectorWindowManager;
        _quizSessionService = quizSessionService;

        UpdateFromState(_stateStore.Current);
    }

    public void Receive(AppStateChangedMessage message)
    {
        UpdateFromState(message.Value);
    }

    private void UpdateFromState(AppState state)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Classes.ReplaceWith(state.Classes.Select(ClassItemViewModel.FromState));
            SelectedClass = Classes.FirstOrDefault(x => x.Id == state.ActiveClassId);

            if (SelectedClass is not null)
            {
                var resolvedUnit = SelectedClass.Units.FirstOrDefault(u => u.Id == state.ActiveUnitId);
                SelectedUnit = resolvedUnit ?? SelectedClass.Units.FirstOrDefault();
            }
            else
            {
                SelectedUnit = null;
            }

            IsAnswerRevealEnabled = state.IsAnswerRevealEnabled;
            IsProjectorFrozen = state.IsProjectorFrozen;

            AnswerRevealButtonText = state.IsAnswerRevealEnabled ? "Hide Answers" : "Reveal Answers";
            FreezeButtonText = state.IsProjectorFrozen ? "Unfreeze Projector" : "Freeze Projector";
            AnswerRevealStatus = state.IsAnswerRevealEnabled ? "Visible" : "Hidden";
            RefreshProjectorStatus();

            var activeClass = state.Classes.FirstOrDefault(x => x.Id == state.ActiveClassId);
            if (activeClass is not null)
            {
                Leaderboard.ReplaceWith(activeClass.Groups.OrderByDescending(x => x.Score).Select(GroupScoreItem.FromState));
            }
            else
            {
                Leaderboard.Clear();
            }

            QuizQuestions.ReplaceWith(state.Quiz.Questions.Select(QuizQuestionViewModel.FromState));
            CurrentQuestion = state.Quiz.CurrentQuestion is null ? null : QuizQuestionViewModel.FromState(state.Quiz.CurrentQuestion);

            TotalQuestions = state.Quiz.TotalQuestions;
            CurrentQuestionNumber = state.Quiz.TotalQuestions == 0
                ? 0
                : Math.Min(state.Quiz.CurrentQuestionIndex + 1, state.Quiz.TotalQuestions);

            UpdateQuizStateFlags(state);
        });
    }

    private void UpdateQuizStateFlags(AppState state)
    {
        IsQuizActive = state.ActiveMode == LessonMode.Quiz && state.Quiz.UnitId.HasValue && state.Quiz.TotalQuestions > 0;
        CanAdvanceQuestion = IsQuizActive && state.Quiz.CurrentQuestionIndex + 1 < state.Quiz.TotalQuestions;
        CanEndQuiz = IsQuizActive;

        var unit = SelectedUnit ?? SelectedClass?.Units.FirstOrDefault(u => u.Id == state.ActiveUnitId);
        CanStartQuiz = !IsQuizActive
            && !IsQuizLoading
            && unit is not null
            && unit.QuestionCount > 0;

        if (!IsQuizActive)
        {
            QuizStatus = unit is null
                ? "Select a unit to begin a quiz"
                : unit.QuestionCount == 0
                    ? $"Unit '{unit.Name}' has no questions"
                    : $"Ready to quiz {unit.Name}";
        }
        else
        {
            QuizStatus = $"Question {Math.Max(1, CurrentQuestionNumber)} of {Math.Max(1, TotalQuestions)}";
        }
    }

    partial void OnIsQuizLoadingChanged(bool value)
    {
        UpdateQuizStateFlags(_stateStore.Current);
    }

    [RelayCommand]
    private void ToggleAnswerReveal()
    {
        var newValue = !IsAnswerRevealEnabled;
        _stateStore.Update(builder =>
        {
            builder.IsAnswerRevealEnabled = newValue;
            return builder;
        });
        _projectorWindowManager.RevealAnswers(newValue);
        RefreshProjectorStatus();
    }

    [RelayCommand]
    private void ToggleFreeze()
    {
        var newValue = !IsProjectorFrozen;
        _stateStore.Update(builder =>
        {
            builder.IsProjectorFrozen = newValue;
            return builder;
        });
        _projectorWindowManager.ToggleFreeze(newValue);
        RefreshProjectorStatus();
    }

    [RelayCommand]
    private void OpenProjector()
    {
        _projectorWindowManager.EnsureProjectorWindow();
        RefreshProjectorStatus();
    }

    [RelayCommand]
    private void ToggleProjectorFullScreen()
    {
        var newValue = !_projectorWindowManager.IsFullScreen;
        _projectorWindowManager.SetFullScreen(newValue);
        RefreshProjectorStatus();
    }

    [RelayCommand]
    private async Task StartQuizAsync()
    {
        if (SelectedClass is null || SelectedUnit is null)
        {
            return;
        }

        try
        {
            IsQuizLoading = true;
            var quizData = await _quizSessionService.LoadQuizAsync(SelectedClass.Id, SelectedUnit.Id, CancellationToken.None);

            var questionStates = quizData.Questions
                .Select(q =>
                {
                    var options = q.Options
                        .Select(option => new QuizOptionState(
                            option,
                            string.Equals(option, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase)))
                        .ToImmutableList();

                    return new QuizQuestionState(q.QuestionId, q.Prompt, options);
                })
                .ToImmutableList();

            if (questionStates.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    $"Unit '{SelectedUnit.Name}' has no quiz questions.",
                    "Quiz Unavailable",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            var initialQuestion = questionStates[0];
            var quizState = new QuizState(
                quizData.UnitId,
                0,
                questionStates.Count,
                initialQuestion,
                questionStates);

            _stateStore.Update(builder =>
            {
                builder.ActiveMode = LessonMode.Quiz;
                builder.ActiveUnitId = SelectedUnit.Id;
                builder.IsAnswerRevealEnabled = false;
                builder.IsProjectorFrozen = false;
                builder.WithQuizState(quizState);
                return builder;
            });

            _projectorWindowManager.RevealAnswers(false);
            UpdateQuizStateFlags(_stateStore.Current);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                ex.Message,
                "Failed to load quiz",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsQuizLoading = false;
        }
    }

    [RelayCommand]
    private void NextQuestion()
    {
        var snapshot = _stateStore.Current;
        if (!CanAdvanceQuestion || snapshot.Quiz.TotalQuestions == 0)
        {
            return;
        }

        var quiz = snapshot.Quiz;
        var nextIndex = quiz.CurrentQuestionIndex + 1;
        if (nextIndex >= quiz.TotalQuestions)
        {
            return;
        }

        var nextQuestion = quiz.Questions[nextIndex];
        var updatedQuiz = new QuizState(
            quiz.UnitId,
            nextIndex,
            quiz.TotalQuestions,
            nextQuestion,
            quiz.Questions);

        _stateStore.Update(builder =>
        {
            builder.IsAnswerRevealEnabled = false;
            builder.ActiveMode = LessonMode.Quiz;
            builder.WithQuizState(updatedQuiz);
            return builder;
        });

        _projectorWindowManager.RevealAnswers(false);
        UpdateQuizStateFlags(_stateStore.Current);
    }

    [RelayCommand]
    private void EndQuiz()
    {
        var snapshot = _stateStore.Current;
        if (!IsQuizActive)
        {
            return;
        }

        _stateStore.Update(builder =>
        {
            builder.IsAnswerRevealEnabled = false;
            builder.ActiveMode = LessonMode.Result;
            builder.WithQuizState(QuizState.Empty);
            return builder;
        });

        _projectorWindowManager.RevealAnswers(false);
        UpdateQuizStateFlags(_stateStore.Current);
    }

    partial void OnSelectedClassChanged(ClassItemViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        var current = _stateStore.Current;
        if (current.ActiveClassId == value.Id)
        {
            return;
        }

        var firstUnitId = value.Units.FirstOrDefault()?.Id;

        _stateStore.Update(builder =>
        {
            builder.ActiveClassId = value.Id;
            builder.ActiveUnitId = firstUnitId;
            builder.ActiveMode = LessonMode.Home;
            builder.IsAnswerRevealEnabled = false;
            builder.WithQuizState(QuizState.Empty);
            return builder;
        });

        _projectorWindowManager.RevealAnswers(false);
    }

    partial void OnSelectedUnitChanged(UnitSummary? value)
    {
        if (value is null)
        {
            UpdateQuizStateFlags(_stateStore.Current);
            return;
        }

        if (_stateStore.Current.ActiveUnitId == value.Id)
        {
            UpdateQuizStateFlags(_stateStore.Current);
            return;
        }

        _stateStore.Update(builder =>
        {
            builder.ActiveUnitId = value.Id;
            builder.ActiveMode = LessonMode.Home;
            builder.IsAnswerRevealEnabled = false;
            builder.WithQuizState(QuizState.Empty);
            return builder;
        });

        _projectorWindowManager.RevealAnswers(false);
        UpdateQuizStateFlags(_stateStore.Current);
    }

    private void RefreshProjectorStatus()
    {
        if (!_projectorWindowManager.IsProjectorVisible)
        {
            ProjectorStatus = "Closed";
            ProjectorFullScreenButtonText = "Go Full Screen";
            return;
        }

        var fullScreenSuffix = _projectorWindowManager.IsFullScreen ? " (Full Screen)" : string.Empty;
        ProjectorStatus = IsProjectorFrozen ? $"Frozen{fullScreenSuffix}" : $"Active{fullScreenSuffix}";
        ProjectorFullScreenButtonText = _projectorWindowManager.IsFullScreen ? "Exit Full Screen" : "Go Full Screen";
    }
}

public sealed class ClassItemViewModel
{
    public Guid Id { get; }
    public string Name { get; }
    public int TotalScore { get; }
    public IReadOnlyList<UnitSummary> Units { get; }

    private ClassItemViewModel(Guid id, string name, int totalScore, IReadOnlyList<UnitSummary> units)
    {
        Id = id;
        Name = name;
        TotalScore = totalScore;
        Units = units;
    }

    public static ClassItemViewModel FromState(ClassState state)
    {
        var totalScore = state.Groups.Sum(group => group.Score);
        return new ClassItemViewModel(state.Id, state.Name, totalScore, state.Units);
    }
}



