using System;

namespace Ozge.App.Infrastructure;

public interface ISoundSettingsService
{
    SoundSettings Current { get; }

    event EventHandler<SoundSettings>? SettingsChanged;

    SoundSettings Load();

    void Update(Func<SoundSettings, SoundSettings> updater);
}

