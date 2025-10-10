using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace Ozge.App.Infrastructure;

public sealed class SoundEffectPlayer : ISoundEffectPlayer, IDisposable
{
    private readonly ISoundSettingsService _settingsService;
    private readonly ILogger<SoundEffectPlayer> _logger;
    private readonly object _playbackLock = new();

    private WaveOutEvent? _activeOutput;
    private AudioFileReader? _activeReader;

    public SoundEffectPlayer(
        ISoundSettingsService settingsService,
        ILogger<SoundEffectPlayer> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    public Task PlayCorrectAsync() => PlayInternalAsync(_settingsService.Current.CorrectSoundPath);

    public Task PlayIncorrectAsync() => PlayInternalAsync(_settingsService.Current.IncorrectSoundPath);

    public Task PlayCelebrationAsync() => PlayInternalAsync(_settingsService.Current.CelebrationSoundPath);

    public Task PreviewAsync(string? filePath) => PlayInternalAsync(filePath);

    private Task PlayInternalAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.CompletedTask;
        }

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Sound file not found at {Path}", filePath);
            return Task.CompletedTask;
        }

        return Task.Run(() =>
        {
            try
            {
                lock (_playbackLock)
                {
                    StopPlayback();

                    _activeReader = new AudioFileReader(filePath);
                    _activeOutput = new WaveOutEvent();
                    _activeOutput.Init(_activeReader);
                    _activeOutput.PlaybackStopped += OnPlaybackStopped;
                    _activeOutput.Play();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sound playback failed for {Path}", filePath);
            }
        });
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        lock (_playbackLock)
        {
            StopPlayback();
        }
    }

    private void StopPlayback()
    {
        _activeOutput?.Stop();
        _activeOutput?.Dispose();
        _activeOutput = null;

        _activeReader?.Dispose();
        _activeReader = null;
    }

    public void Dispose()
    {
        lock (_playbackLock)
        {
            StopPlayback();
        }
    }
}

