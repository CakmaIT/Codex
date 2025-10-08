using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Ozge.Core;
using Ozge.Core.Contracts;
using SkiaSharp;

namespace Ozge.App.Infrastructure;

public sealed class DependencyCheckService
{
    private readonly IFileSystem _fileSystem;
    private readonly IAssetProvisioner _assetProvisioner;
    private readonly ILogger<DependencyCheckService> _logger;

    public DependencyCheckService(
        IFileSystem fileSystem,
        IAssetProvisioner assetProvisioner,
        ILogger<DependencyCheckService> logger)
    {
        _fileSystem = fileSystem;
        _assetProvisioner = assetProvisioner;
        _logger = logger;
    }

    public DependencyCheckResult Validate()
    {
        var messages = new List<string>();

        if (!CheckSqlite(messages) | !CheckTessData(messages) | !CheckSkiaSharp(messages))
        {
            return new DependencyCheckResult(false, messages);
        }

        _logger.LogInformation("All dependencies validated.");
        return DependencyCheckResult.Passed;
    }

    private bool CheckSqlite(ICollection<string> messages)
    {
        var sqlitePath = Path.Combine(AppContext.BaseDirectory, "e_sqlite3.dll");
        if (_fileSystem.FileExists(sqlitePath))
        {
            _logger.LogInformation("Detected SQLite native driver at {Path}", sqlitePath);
            return true;
        }

        var bundledPath = Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native", "e_sqlite3.dll");
        if (_fileSystem.FileExists(bundledPath))
        {
            try
            {
                _fileSystem.CopyFile(bundledPath, sqlitePath, overwrite: true);
                _logger.LogInformation("Copied e_sqlite3.dll from runtimes folder to {Destination}", sqlitePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy e_sqlite3.dll from {Source}", bundledPath);
            }
        }

        messages.Add("SQLite native driver (e_sqlite3.dll) missing in the application folder.");
        _logger.LogError("Missing e_sqlite3.dll at {Path}", sqlitePath);
        return false;
    }

    private bool CheckTessData(ICollection<string> messages)
    {
        var tessFile = Path.Combine(_assetProvisioner.TessDataDirectory, "eng.traineddata");
        if (_fileSystem.FileExists(tessFile))
        {
            _logger.LogInformation("Detected tessdata file at {Path}", tessFile);
            return true;
        }

        messages.Add("OCR data file 'eng.traineddata' not found under %LOCALAPPDATA%\\Ozge2\\tessdata.");
        _logger.LogError("Tesseract data file missing at {Path}", tessFile);
        return false;
    }

    private bool CheckSkiaSharp(ICollection<string> messages)
    {
        try
        {
            using var bitmap = new SKBitmap(1, 1);
            return true;
        }
        catch (DllNotFoundException ex)
        {
            const string guidance = "SkiaSharp native dependency missing (Visual C++ 2015-2022 Redistributable). Install VC_redist.x64.exe and restart the app.";
            messages.Add(guidance);
            _logger.LogError(ex, "SkiaSharp native dependency missing.");
            return false;
        }
        catch (Exception ex)
        {
            messages.Add("SkiaSharp initialisation failed. Check native dependencies.");
            _logger.LogError(ex, "Unexpected error while validating SkiaSharp dependencies.");
            return false;
        }
    }
}

public sealed class DependencyCheckResult
{
    public static DependencyCheckResult Passed { get; } = new(true, Array.Empty<string>());

    public DependencyCheckResult(bool success, IReadOnlyList<string> messages)
    {
        Success = success;
        Messages = messages;
    }

    public bool Success { get; }

    public IReadOnlyList<string> Messages { get; }

    public override string ToString()
    {
        if (Success || Messages.Count == 0)
        {
            return "All good";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Missing components detected:");
        foreach (var message in Messages)
        {
            builder.Append("- ");
            builder.AppendLine(message);
        }

        return builder.ToString();
    }
}
