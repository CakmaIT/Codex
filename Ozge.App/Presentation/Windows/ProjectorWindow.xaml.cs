using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Ozge.App.Presentation.ViewModels;

namespace Ozge.App.Presentation.Windows;

public partial class ProjectorWindow : Window
{
    private readonly ProjectorViewModel _viewModel;
    private readonly MediaPlayer _celebrationPlayer = new();
    private string? _currentCelebrationSoundPath;

    public ProjectorWindow(ProjectorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _celebrationPlayer.MediaEnded += (_, _) => _celebrationPlayer.Stop();
        UpdateCelebrationSound();
        if (_viewModel.ShowCelebration)
        {
            PlayCelebrationSound();
        }
        Closed += OnClosed;
    }

    public void SetFreezeOverlay(bool isFrozen)
    {
        FreezeOverlay.Visibility = isFrozen ? Visibility.Visible : Visibility.Collapsed;
    }

    public void SetAnswerReveal(bool reveal)
    {
        AnswerOverlay.Visibility = reveal ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.Equals(e.PropertyName, nameof(ProjectorViewModel.CelebrationSoundPath), StringComparison.Ordinal))
        {
            UpdateCelebrationSound();
            return;
        }

        if (string.Equals(e.PropertyName, nameof(ProjectorViewModel.ShowCelebration), StringComparison.Ordinal))
        {
            if (_viewModel.ShowCelebration)
            {
                PlayCelebrationSound();
            }
            else
            {
                _celebrationPlayer.Stop();
            }
        }
    }

    private void UpdateCelebrationSound()
    {
        var path = _viewModel.CelebrationSoundPath;
        _currentCelebrationSoundPath = string.IsNullOrWhiteSpace(path) ? null : path;

        if (_currentCelebrationSoundPath is null)
        {
            _celebrationPlayer.Stop();
        }
        else if (_viewModel.ShowCelebration)
        {
            PlayCelebrationSound();
        }
    }

    private void PlayCelebrationSound()
    {
        if (_currentCelebrationSoundPath is null || !File.Exists(_currentCelebrationSoundPath))
        {
            return;
        }

        try
        {
            _celebrationPlayer.Stop();
            _celebrationPlayer.Open(new Uri(_currentCelebrationSoundPath));
            _celebrationPlayer.Position = TimeSpan.Zero;
            _celebrationPlayer.Play();
        }
        catch
        {
            // Ignored: playback errors should not crash the projector window.
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _celebrationPlayer.Stop();
        _celebrationPlayer.Close();
    }
}
