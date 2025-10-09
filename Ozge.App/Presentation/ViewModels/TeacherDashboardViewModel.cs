using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Ozge.App.Presentation.Messaging;
using Ozge.Core.Contracts;
using Ozge.Core.Domain.Enums;
using Ozge.Core.Models;
using Ozge.Core.State;

namespace Ozge.App.Presentation.ViewModels;

public sealed partial class TeacherDashboardViewModel : ViewModelBase, IRecipient<AppStateChangedMessage>
{
    private readonly IAppStateStore _stateStore;
    private readonly IProjectorWindowManager _projectorWindowManager;
    private readonly IQuizSessionService _quizSessionService;
    private readonly IQuestionImportService _questionImportService;
    private readonly IDisplayService _displayService;

    public ObservableCollection<ClassItemViewModel> Classes { get; } = new();
    public ObservableCollection<GroupScoreItem> Leaderboard { get; } = new();
    public ObservableCollection<QuizQuestionViewModel> QuizQuestions { get; } = new();
    public ObservableCollection<QuestionBankItemViewModel> QuestionBank { get; } = new();
    public ObservableCollection<ProjectorDisplayOptionViewModel> ProjectorDisplays { get; } = new();
    public ObservableCollection<DashboardMenuOptionViewModel> DashboardMenus { get; } = new();

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
    private bool isImportingQuestions;

    [ObservableProperty]
    private bool isQuestionBankLoading;

    [ObservableProperty]
    private bool canStartQuiz;

    [ObservableProperty]
    private bool canAdvanceQuestion;

    [ObservableProperty]
    private bool canGoBackQuestion;

    [ObservableProperty]
    private bool canEndQuiz;

    [ObservableProperty]
    private int currentQuestionNumber;

    [ObservableProperty]
    private int totalQuestions;

    [ObservableProperty]
    private string quizStatus = "Quiz baslamadi";

    [ObservableProperty]
    private string answerRevealButtonText = "Cevaplari Goster";

    [ObservableProperty]
    private string freezeButtonText = "Projeksiyonu Dondur";

    [ObservableProperty]
    private string projectorStatus = "Kapali";

    [ObservableProperty]
    private string answerRevealStatus = "Gizli";

    [ObservableProperty]
    private string projectorFullScreenButtonText = "Tam Ekran Ac";

    [ObservableProperty]
    private double quizProgressValue;

    [ObservableProperty]
    private string quizProgressText = "0 / 0";

    [ObservableProperty]
    private string currentQuestionTitle = "Quiz baslamadi";

    [ObservableProperty]
    private string nextQuestionButtonText = "Sonraki Soru";

    [ObservableProperty]
    private string previousQuestionButtonText = "Onceki Soru";

    [ObservableProperty]
    private string questionBankStatus = "Soru bankasi yuklenmedi";

    [ObservableProperty]
    private ProjectorDisplayOptionViewModel? selectedProjectorDisplay;

    [ObservableProperty]
    private DashboardMenuOptionViewModel? selectedMenu;

    [ObservableProperty]
    private bool isQuizMenuActive;

    [ObservableProperty]
    private bool isQuestionBankMenuActive;

    [ObservableProperty]
    private bool isImportMenuActive;

    [ObservableProperty]
    private bool isPeopleMenuActive;

    [ObservableProperty]
    private string newClassName = string.Empty;

    [ObservableProperty]
    private string newStudentName = string.Empty;

    [ObservableProperty]
    private string newStudentSeat = string.Empty;

    [ObservableProperty]
    private ClassItemViewModel? selectedClassForNewStudent;

    [ObservableProperty]
    private string newTeamName = string.Empty;

    [ObservableProperty]
    private string newTeamAvatar = "🏆";

    [ObservableProperty]
    private ClassItemViewModel? selectedClassForNewTeam;

    [ObservableProperty]
    private string autoTeamCountText = "2";

    [ObservableProperty]
    private ClassItemViewModel? selectedClassForAutoTeams;

    [ObservableProperty]
    private string peopleStatusMessage = string.Empty;

    public TeacherDashboardViewModel(
        IAppStateStore stateStore,
        IProjectorWindowManager projectorWindowManager,
        IQuizSessionService quizSessionService,
        IQuestionImportService questionImportService,
        IDisplayService displayService,
        IMessenger messenger)
        : base(messenger)
    {
        _stateStore = stateStore;
        _projectorWindowManager = projectorWindowManager;
        _quizSessionService = quizSessionService;
        _questionImportService = questionImportService;
        _displayService = displayService;

        LoadProjectorDisplays();
        InitializeMenus();
        UpdateFromState(_stateStore.Current);
    }

    private void LoadProjectorDisplays()
    {
        var displays = _displayService
            .GetDisplays()
            .OrderBy(display => display.IsPrimary ? 0 : 1)
            .ThenBy(display => display.FriendlyName, StringComparer.CurrentCultureIgnoreCase)
            .Select((display, index) => ProjectorDisplayOptionViewModel.FromDescriptor(display, index + 1))
            .ToList();

        ProjectorDisplays.ReplaceWith(displays);
    }

    private void InitializeMenus()
    {
        var menus = new[]
        {
            DashboardMenuOptionViewModel.Create(DashboardMenuKey.QuizControl, "Quiz Kontrol", "Canli oturumu yonet"),
            DashboardMenuOptionViewModel.Create(DashboardMenuKey.QuestionBank, "Soru Bankasi", "Kayitli icerigi goruntule"),
            DashboardMenuOptionViewModel.Create(DashboardMenuKey.Import, "Icerik Aktar", "Yeni sorular ekle"),
            DashboardMenuOptionViewModel.Create(DashboardMenuKey.PeopleManagement, "Kisi Ekleme", "Sinif, ogrenci ve takim islemleri")
        };

        DashboardMenus.ReplaceWith(menus);
        SelectedMenu ??= DashboardMenus.FirstOrDefault();
    }

    public void Receive(AppStateChangedMessage message)
    {
        UpdateFromState(message.Value);
    }

    private void UpdateFromState(AppState state)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var previousStudentClassId = SelectedClassForNewStudent?.Id;
            var previousTeamClassId = SelectedClassForNewTeam?.Id;
            var previousAutoTeamClassId = SelectedClassForAutoTeams?.Id;

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

            var preferredDisplayId = state.PreferredProjectorDisplayId
                ?? _projectorWindowManager.CurrentDisplayId
                ?? _displayService.DefaultProjectorDisplayId;

            ProjectorDisplayOptionViewModel? resolvedDisplay = null;
            if (ProjectorDisplays.Any())
            {
                if (!string.IsNullOrWhiteSpace(preferredDisplayId))
                {
                    resolvedDisplay = ProjectorDisplays.FirstOrDefault(option =>
                        string.Equals(option.Id, preferredDisplayId, StringComparison.OrdinalIgnoreCase));
                }

                resolvedDisplay ??= ProjectorDisplays.FirstOrDefault();
            }

            if (!Equals(SelectedProjectorDisplay, resolvedDisplay))
            {
                SelectedProjectorDisplay = resolvedDisplay;
            }
            else
            {
                _projectorWindowManager.SetTargetDisplay(resolvedDisplay?.Id);
            }

            AnswerRevealButtonText = state.IsAnswerRevealEnabled ? "Cevaplari Gizle" : "Cevaplari Goster";
            FreezeButtonText = state.IsProjectorFrozen ? "Projeksiyonu Ac" : "Projeksiyonu Dondur";
            AnswerRevealStatus = state.IsAnswerRevealEnabled ? "Gorunuyor" : "Gizli";
            RefreshProjectorStatus();

            SelectedClassForNewStudent = previousStudentClassId is null
                ? null
                : Classes.FirstOrDefault(x => x.Id == previousStudentClassId) ?? SelectedClass;
            SelectedClassForNewTeam = previousTeamClassId is null
                ? null
                : Classes.FirstOrDefault(x => x.Id == previousTeamClassId) ?? SelectedClass;
            SelectedClassForAutoTeams = previousAutoTeamClassId is null
                ? null
                : Classes.FirstOrDefault(x => x.Id == previousAutoTeamClassId) ?? SelectedClass;

            if (SelectedClass is not null)
            {
                SelectedClassForNewStudent ??= SelectedClass;
                SelectedClassForNewTeam ??= SelectedClass;
                SelectedClassForAutoTeams ??= SelectedClass;
            }
            else
            {
                SelectedClassForNewStudent = null;
                SelectedClassForNewTeam = null;
                SelectedClassForAutoTeams = null;
            }

            var activeClass = state.Classes.FirstOrDefault(x => x.Id == state.ActiveClassId);
            if (activeClass is not null)
            {
                Leaderboard.ReplaceWith(activeClass.Groups.OrderByDescending(x => x.Score).Select(GroupScoreItem.FromState));
            }
            else
            {
                Leaderboard.Clear();
            }

            var quizQuestionViewModels = state.Quiz.Questions
                .Select((question, index) => QuizQuestionViewModel.FromState(question, index + 1))
                .ToList();
            QuizQuestions.ReplaceWith(quizQuestionViewModels);
            CurrentQuestion = state.Quiz.CurrentQuestion is null
                ? null
                : quizQuestionViewModels.FirstOrDefault(q => q.QuestionId == state.Quiz.CurrentQuestion.QuestionId)
                  ?? QuizQuestionViewModel.FromState(state.Quiz.CurrentQuestion, state.Quiz.CurrentQuestionIndex + 1);

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
        var totalQuestions = Math.Max(1, state.Quiz.TotalQuestions);
        var questionNumber = state.Quiz.TotalQuestions == 0 ? 0 : state.Quiz.CurrentQuestionIndex + 1;

        QuizProgressValue = state.Quiz.TotalQuestions == 0
            ? 0
            : Math.Clamp((double)questionNumber / totalQuestions, 0, 1);
        QuizProgressText = state.Quiz.TotalQuestions == 0
            ? "0 / 0"
            : $"{questionNumber} / {state.Quiz.TotalQuestions}";

        var controlsLocked = IsImportingQuestions || IsQuizLoading || IsQuestionBankLoading;

        CanGoBackQuestion = IsQuizActive && !controlsLocked && state.Quiz.CurrentQuestionIndex > 0;
        CanAdvanceQuestion = IsQuizActive && !controlsLocked && state.Quiz.CurrentQuestionIndex + 1 < state.Quiz.TotalQuestions;
        CanEndQuiz = IsQuizActive && !controlsLocked;
        NextQuestionButtonText = !IsQuizActive
            ? "Sonraki Soru"
            : CanAdvanceQuestion
                ? "Sonraki Soru"
                : "Sonuclari Goster";

        var unit = SelectedUnit ?? SelectedClass?.Units.FirstOrDefault(u => u.Id == state.ActiveUnitId);
        CanStartQuiz = !IsQuizActive
            && !IsQuizLoading
            && !IsImportingQuestions
            && unit is not null
            && unit.QuestionCount > 0;

        CurrentQuestionTitle = IsQuizActive
            ? state.Quiz.CurrentQuestion is null
                ? "Quiz devam ediyor"
                : $"{state.Quiz.CurrentQuestionIndex + 1}. soru hazir"
            : unit is null
                ? "Bir sinif ve unite sec"
                : $"{unit.Name} hazir";

        if (!IsQuizActive)
        {
            QuizStatus = unit is null
                ? "Quiz baslatmak icin unite sec"
                : unit.QuestionCount == 0
                    ? $"Unite '{unit.Name}' icin soru yok"
                    : $"{unit.Name} icin quiz hazir";
        }
        else
        {
            QuizStatus = $"Soru {Math.Max(1, CurrentQuestionNumber)} / {Math.Max(1, TotalQuestions)}";
        }
    }

    partial void OnIsQuizLoadingChanged(bool value)
    {
        UpdateQuizStateFlags(_stateStore.Current);
    }

    partial void OnIsImportingQuestionsChanged(bool value)
    {
        UpdateQuizStateFlags(_stateStore.Current);
    }

    partial void OnIsQuestionBankLoadingChanged(bool value)
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
        _projectorWindowManager.SetTargetDisplay(SelectedProjectorDisplay?.Id);
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
                    $"Unite '{SelectedUnit.Name}' icin kayitli soru yok.",
                    "Quiz bulunamadi",
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
                "Quiz yuklenemedi",
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
    private async Task ImportQuestionsAsync()
    {
        if (SelectedClass is null || SelectedUnit is null)
        {
            System.Windows.MessageBox.Show(
                "Once bir sinif ve unite secmelisiniz.",
                "Ic aktarim kullanilamadi",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PDF Dosyalari (*.pdf)|*.pdf",
            Title = "PDF'den Soru Aktar",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            IsImportingQuestions = true;
            var importResult = await _questionImportService.ImportPdfAsync(
                SelectedClass.Id,
                SelectedUnit.Id,
                dialog.FileName,
                CancellationToken.None);

            var messageBuilder = new StringBuilder();
            if (importResult.QuestionsAdded == 0 && importResult.WordsAdded == 0)
            {
                messageBuilder.AppendLine("Yeni soru veya kelime bulunamadi.");
            }
            else
            {
                messageBuilder.AppendLine($"{importResult.QuestionsAdded} soru eklendi.");
                if (importResult.WordsAdded > 0)
                {
                    messageBuilder.AppendLine($"{importResult.WordsAdded} kelime eklendi.");
                }
            }

            foreach (var diagnostic in importResult.Diagnostics)
            {
                messageBuilder.AppendLine($"[{diagnostic.Severity}] {diagnostic.Message}");
            }

            System.Windows.MessageBox.Show(
                messageBuilder.ToString(),
                "PDF iceri aktarma tamamlandi",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            await LoadQuestionBankAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                ex.Message,
                "PDF iceri aktarma hatasi",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsImportingQuestions = false;
            UpdateQuizStateFlags(_stateStore.Current);
        }
    }

    [RelayCommand]
    private async Task LoadQuestionBankAsync()
    {
        if (SelectedClass is null || SelectedUnit is null)
        {
            System.Windows.MessageBox.Show(
                "Once bir sinif ve unite secmelisiniz.",
                "Soru bankasi yuklenemedi",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        if (IsQuestionBankLoading)
        {
            return;
        }

        try
        {
            IsQuestionBankLoading = true;
            QuestionBankStatus = "Soru bankasi yukleniyor...";

            var quizData = await _quizSessionService.LoadQuizAsync(
                SelectedClass.Id,
                SelectedUnit.Id,
                CancellationToken.None);

            QuestionBank.Clear();
            foreach (var question in quizData.Questions)
            {
                QuestionBank.Add(new QuestionBankItemViewModel(
                    question.Prompt,
                    question.CorrectAnswer,
                    question.Options));
            }

            QuestionBankStatus = QuestionBank.Count == 0
                ? "Soru bulunamadi."
                : $"{QuestionBank.Count} soru listelendi.";
        }
        catch (Exception ex)
        {
            QuestionBankStatus = "Soru bankasi yuklenemedi.";
            System.Windows.MessageBox.Show(
                ex.Message,
                "Soru bankasi yuklenemedi",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsQuestionBankLoading = false;
            UpdateQuizStateFlags(_stateStore.Current);
        }
    }

    [RelayCommand]
    private void PreviousQuestion()
    {
        var snapshot = _stateStore.Current;
        if (!CanGoBackQuestion || snapshot.Quiz.TotalQuestions == 0)
        {
            return;
        }

        var quiz = snapshot.Quiz;
        var previousIndex = Math.Max(0, quiz.CurrentQuestionIndex - 1);
        var previousQuestion = quiz.Questions[previousIndex];

        var updatedQuiz = new QuizState(
            quiz.UnitId,
            previousIndex,
            quiz.TotalQuestions,
            previousQuestion,
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
    private void AddQuestion()
    {
        var currentState = _stateStore.Current;
        var activeUnitId = currentState.Quiz.UnitId ?? SelectedUnit?.Id;
        if (activeUnitId is null)
        {
            System.Windows.MessageBox.Show(
                "Once bir sinif ve unite secmelisiniz.",
                "Soru eklenemedi",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        var quiz = currentState.Quiz;
        var newQuestion = new QuizQuestionState(
            Guid.NewGuid(),
            $"Yeni Soru {quiz.TotalQuestions + 1}",
            ImmutableList.Create(
                new QuizOptionState("Secenek A", true),
                new QuizOptionState("Secenek B", false),
                new QuizOptionState("Secenek C", false),
                new QuizOptionState("Secenek D", false)));

        var updatedQuestions = quiz.Questions.Add(newQuestion);
        var newIndex = updatedQuestions.Count - 1;

        var updatedQuiz = new QuizState(
            activeUnitId,
            newIndex,
            updatedQuestions.Count,
            newQuestion,
            updatedQuestions);

        _stateStore.Update(builder =>
        {
            builder.ActiveMode = LessonMode.Quiz;
            builder.ActiveUnitId = activeUnitId;
            builder.IsAnswerRevealEnabled = false;
            builder.IsProjectorFrozen = false;
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

    [RelayCommand]
    private void AddClass()
    {
        if (string.IsNullOrWhiteSpace(NewClassName))
        {
            PeopleStatusMessage = "Sinif adi bos olamaz.";
            return;
        }

        var trimmedName = NewClassName.Trim();
        var newClass = ClassState.Empty(Guid.NewGuid(), trimmedName);

        _stateStore.Update(builder =>
        {
            builder.UpdateClass(newClass);

            if (builder.ActiveClassId == Guid.Empty)
            {
                builder.ActiveClassId = newClass.Id;
            }

            return builder;
        });

        NewClassName = string.Empty;
        PeopleStatusMessage = $"Yeni sinif eklendi: {trimmedName}";
    }

    [RelayCommand]
    private void AddStudent()
    {
        var targetClass = SelectedClassForNewStudent ?? SelectedClass;

        if (targetClass is null)
        {
            PeopleStatusMessage = "Önce bir sinif secmelisiniz.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewStudentName))
        {
            PeopleStatusMessage = "Öğrenci adi bos olamaz.";
            return;
        }

        var currentState = _stateStore.Current;
        var classState = currentState.Classes.FirstOrDefault(c => c.Id == targetClass.Id);
        if (classState is null)
        {
            PeopleStatusMessage = "Secilen sinif bulunamadi.";
            return;
        }

        var seat = string.IsNullOrWhiteSpace(NewStudentSeat)
            ? (classState.Students.Count + 1).ToString()
            : NewStudentSeat.Trim();

        var student = new StudentState(
            Guid.NewGuid(),
            NewStudentName.Trim(),
            seat,
            true,
            true);

        var updatedClass = classState with { Students = classState.Students.Add(student) };

        _stateStore.Update(builder =>
        {
            builder.UpdateClass(updatedClass);
            builder.ActiveClassId = updatedClass.Id;
            return builder;
        });

        NewStudentName = string.Empty;
        NewStudentSeat = string.Empty;
        PeopleStatusMessage = $"Öğrenci eklendi: {student.Name}";
    }

    [RelayCommand]
    private void AddTeam()
    {
        var targetClass = SelectedClassForNewTeam ?? SelectedClass;

        if (targetClass is null)
        {
            PeopleStatusMessage = "Takim eklemek icin bir sinif secin.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewTeamName))
        {
            PeopleStatusMessage = "Takim adi bos olamaz.";
            return;
        }

        var currentState = _stateStore.Current;
        var classState = currentState.Classes.FirstOrDefault(c => c.Id == targetClass.Id);
        if (classState is null)
        {
            PeopleStatusMessage = "Secilen sinif bulunamadi.";
            return;
        }

        var avatar = string.IsNullOrWhiteSpace(NewTeamAvatar) ? "🏆" : NewTeamAvatar.Trim();

        var group = new GroupState(
            Guid.NewGuid(),
            NewTeamName.Trim(),
            avatar,
            0,
            0,
            false,
            DateTimeOffset.UtcNow);

        var updatedClass = classState with { Groups = classState.Groups.Add(group) };

        _stateStore.Update(builder =>
        {
            builder.UpdateClass(updatedClass);
            builder.ActiveClassId = updatedClass.Id;
            return builder;
        });

        NewTeamName = string.Empty;
        NewTeamAvatar = avatar;
        PeopleStatusMessage = $"Takim eklendi: {group.Name}";
    }

    [RelayCommand]
    private void AutoCreateTeams()
    {
        var targetClass = SelectedClassForAutoTeams ?? SelectedClass;

        if (targetClass is null)
        {
            PeopleStatusMessage = "Önce otomatik takımlar icin bir sinif secin.";
            return;
        }

        if (!int.TryParse(AutoTeamCountText, out var count) || count <= 0)
        {
            PeopleStatusMessage = "Gecerli bir takim sayisi girin.";
            return;
        }

        var currentState = _stateStore.Current;
        var classState = currentState.Classes.FirstOrDefault(c => c.Id == targetClass.Id);
        if (classState is null)
        {
            PeopleStatusMessage = "Secilen sinif bulunamadi.";
            return;
        }

        var avatars = new[] { "🐯", "🦅", "🐳", "🛡️", "🚀", "🎯", "🐉", "🦁" };
        var groups = Enumerable.Range(1, count)
            .Select(i => new GroupState(
                Guid.NewGuid(),
                $"Takim {i}",
                avatars[(i - 1) % avatars.Length],
                0,
                0,
                false,
                DateTimeOffset.UtcNow))
            .ToImmutableList();

        var updatedClass = classState with { Groups = groups };

        _stateStore.Update(builder =>
        {
            builder.UpdateClass(updatedClass);
            builder.ActiveClassId = updatedClass.Id;
            return builder;
        });

        PeopleStatusMessage = $"Toplam {count} takim olusturuldu.";
    }

    partial void OnSelectedMenuChanged(DashboardMenuOptionViewModel? value)
    {
        var key = value?.Key ?? DashboardMenuKey.QuizControl;

        IsQuizMenuActive = key == DashboardMenuKey.QuizControl;
        IsQuestionBankMenuActive = key == DashboardMenuKey.QuestionBank;
        IsImportMenuActive = key == DashboardMenuKey.Import;
        IsPeopleMenuActive = key == DashboardMenuKey.PeopleManagement;

        if (IsPeopleMenuActive)
        {
            PeopleStatusMessage = string.Empty;
            SelectedClassForNewStudent ??= SelectedClass;
            SelectedClassForNewTeam ??= SelectedClass;
            SelectedClassForAutoTeams ??= SelectedClass;
        }

        if (IsQuestionBankMenuActive &&
            QuestionBank.Count == 0 &&
            !IsQuestionBankLoading &&
            SelectedClass is not null &&
            SelectedUnit is not null)
        {
            if (LoadQuestionBankCommand.CanExecute(null))
            {
                LoadQuestionBankCommand.Execute(null);
            }
        }

        RefreshProjectorStatus();
    }

    partial void OnSelectedProjectorDisplayChanged(ProjectorDisplayOptionViewModel? value)
    {
        var targetId = value?.Id;
        _projectorWindowManager.SetTargetDisplay(targetId);

        if (_stateStore.Current.PreferredProjectorDisplayId == targetId)
        {
            RefreshProjectorStatus();
            return;
        }

        _stateStore.Update(builder =>
        {
            builder.PreferredProjectorDisplayId = targetId;
            return builder;
        });

        RefreshProjectorStatus();
    }

    partial void OnSelectedClassChanged(ClassItemViewModel? value)
    {
        if (value is null)
        {
            QuestionBank.Clear();
            QuestionBankStatus = "Soru bankasi yuklenmedi";
            return;
        }

        var current = _stateStore.Current;
        if (current.ActiveClassId == value.Id)
        {
            return;
        }

        QuestionBank.Clear();
        QuestionBankStatus = "Soru bankasi yuklenmedi";

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
            QuestionBank.Clear();
            QuestionBankStatus = "Soru bankasi yuklenmedi";
            UpdateQuizStateFlags(_stateStore.Current);
            return;
        }

        if (_stateStore.Current.ActiveUnitId == value.Id)
        {
            UpdateQuizStateFlags(_stateStore.Current);
            return;
        }

        QuestionBank.Clear();
        QuestionBankStatus = "Soru bankasi yuklenmedi";
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
        var displaySummary = SelectedProjectorDisplay?.Summary ?? "Ekran secilmedi";

        if (!_projectorWindowManager.IsProjectorVisible)
        {
            ProjectorStatus = $"Kapali - {displaySummary}";
            ProjectorFullScreenButtonText = "Tam Ekran Ac";
            return;
        }

        var fullScreenSuffix = _projectorWindowManager.IsFullScreen ? " (Tam Ekran)" : string.Empty;
        var prefix = IsProjectorFrozen ? "Donduruldu" : "Aktif";
        ProjectorStatus = $"{prefix}{fullScreenSuffix} - {displaySummary}";
        ProjectorFullScreenButtonText = _projectorWindowManager.IsFullScreen ? "Tam Ekrandan Cik" : "Tam Ekran Ac";
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

public sealed class ProjectorDisplayOptionViewModel
{
    private ProjectorDisplayOptionViewModel(string id, string displayLabel, string resolutionLabel, bool isPrimary)
    {
        Id = id;
        DisplayLabel = displayLabel;
        ResolutionLabel = resolutionLabel;
        IsPrimary = isPrimary;
    }

    public string Id { get; }
    public string DisplayLabel { get; }
    public string ResolutionLabel { get; }
    public bool IsPrimary { get; }

    public string Summary => $"{DisplayLabel} - {ResolutionLabel}";

    public static ProjectorDisplayOptionViewModel FromDescriptor(DisplayDescriptor descriptor, int order)
    {
        var resolution = $"{(int)Math.Round(descriptor.Width)}x{(int)Math.Round(descriptor.Height)}";
        var fallbackName = descriptor.IsPrimary ? "Ana Ekran" : $"Ekran {order}";
        var friendlyName = string.IsNullOrWhiteSpace(descriptor.FriendlyName)
            ? fallbackName
            : descriptor.FriendlyName;

        if (string.Equals(friendlyName, descriptor.Id, StringComparison.OrdinalIgnoreCase))
        {
            friendlyName = fallbackName;
        }

        var displayLabel = descriptor.IsPrimary ? $"{friendlyName} (Ana)" : friendlyName;

        return new ProjectorDisplayOptionViewModel(descriptor.Id, displayLabel, resolution, descriptor.IsPrimary);
    }

    public override string ToString() => Summary;
}

public enum DashboardMenuKey
{
    QuizControl,
    QuestionBank,
    Import,
    PeopleManagement
}

public sealed class DashboardMenuOptionViewModel
{
    private DashboardMenuOptionViewModel(DashboardMenuKey key, string title, string description)
    {
        Key = key;
        Title = title;
        Description = description;
    }

    public DashboardMenuKey Key { get; }
    public string Title { get; }
    public string Description { get; }

    public static DashboardMenuOptionViewModel Create(DashboardMenuKey key, string title, string description) =>
        new(key, title, description);

    public override string ToString() => Title;
}




