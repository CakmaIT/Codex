using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ozge.Core;

namespace Ozge.App.Infrastructure;

public sealed class SoundSettingsService : ISoundSettingsService
{
    private readonly ILogger<SoundSettingsService> _logger;
    private readonly string _settingsPath;
    private readonly object _sync = new();
    private SoundSettings _current;

    public SoundSettingsService(ILogger<SoundSettingsService> logger)
    {
        _logger = logger;
        var root = AppConstants.GetLocalAppDataRoot();
        Directory.CreateDirectory(root);
        _settingsPath = Path.Combine(root, AppConstants.SoundSettingsFileName);
        _current = LoadInternal();
    }

    public SoundSettings Current
    {
        get
        {
            lock (_sync)
            {
                return _current;
            }
        }
    }

    public event EventHandler<SoundSettings>? SettingsChanged;

    public SoundSettings Load()
    {
        lock (_sync)
        {
            _current = LoadInternal();
            return _current;
        }
    }

    public void Update(Func<SoundSettings, SoundSettings> updater)
    {
        if (updater is null)
        {
            throw new ArgumentNullException(nameof(updater));
        }

        SoundSettings updated;
        lock (_sync)
        {
            updated = updater(_current);
            SaveInternal(updated);
            _current = updated;
        }

        SettingsChanged?.Invoke(this, updated);
    }

    private SoundSettings LoadInternal()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return SoundSettings.Default;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<SoundSettings>(json);
            return settings ?? SoundSettings.Default;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sound settings could not be loaded, using defaults.");
            return SoundSettings.Default;
        }
    }

    private void SaveInternal(SoundSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sound settings could not be persisted.");
        }
    }
}

