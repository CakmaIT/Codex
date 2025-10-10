using System;
using System.IO;

namespace Ozge.Core;

public static class AppConstants
{
    public const string ApplicationName = "Özge2";
    public const string AppFolderName = "Ozge2";
    public const string DatabaseFileName = "ozge2.db";
    public const string SnapshotsFolderName = "Snapshots";
    public const string BackupsFolderName = "Backups";
    public const string LogsFolderName = "Logs";
    public const string TessDataFolderName = "tessdata";
    public const string SoundSettingsFileName = "soundsettings.json";

    public static string GetLocalAppDataRoot()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, AppFolderName);
    }
}
