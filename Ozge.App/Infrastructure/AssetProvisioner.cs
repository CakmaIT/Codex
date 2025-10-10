using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Ozge.Assets;
using Ozge.Core;
using Ozge.Core.Contracts;

namespace Ozge.App.Infrastructure;

public sealed class AssetProvisioner : IAssetProvisioner
{
    private const long MinimumExpectedTessdataSizeBytes = 1_000_000;

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<AssetProvisioner> _logger;

    public AssetProvisioner(IFileSystem fileSystem, ILogger<AssetProvisioner> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public string TessDataDirectory => Path.Combine(AppConstants.GetLocalAppDataRoot(), AppConstants.TessDataFolderName);

    public async Task EnsureAssetsAsync(CancellationToken cancellationToken)
    {
        _fileSystem.CreateDirectory(TessDataDirectory);

        var tessdataTarget = Path.Combine(TessDataDirectory, "eng.traineddata");
        var targetInfo = new FileInfo(tessdataTarget);
        var needsProvision = !targetInfo.Exists || targetInfo.Length < MinimumExpectedTessdataSizeBytes;

        if (!needsProvision)
        {
            return;
        }

        if (await TryCopyFromLocalPackageAsync(tessdataTarget, cancellationToken))
        {
            WarnIfPlaceholder(tessdataTarget);
            return;
        }

        if (await TryExtractFromResourceAsync(tessdataTarget, cancellationToken))
        {
            WarnIfPlaceholder(tessdataTarget);
            return;
        }

        _logger.LogWarning("Could not locate tessdata assets. OCR accuracy may be reduced until a full language pack is supplied.");
        WarnIfPlaceholder(tessdataTarget);
    }

    private async Task<bool> TryCopyFromLocalPackageAsync(string destination, CancellationToken cancellationToken)
    {
        var localTessdataFolder = Path.Combine(AppContext.BaseDirectory, "tessdata");
        if (!_fileSystem.DirectoryExists(localTessdataFolder))
        {
            return false;
        }

        var compressedPath = Path.Combine(localTessdataFolder, "eng.traineddata.gz");
        if (_fileSystem.FileExists(compressedPath))
        {
            try
            {
                using var compressedStream = _fileSystem.OpenRead(compressedPath);
                using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                using var destinationStream = _fileSystem.OpenWrite(destination);
                await gzipStream.CopyToAsync(destinationStream, cancellationToken);
                _logger.LogInformation("Extracted tessdata from compressed package {Source}", compressedPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract tessdata from {Source}", compressedPath);
                try
                {
                    File.Delete(destination);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogDebug(cleanupEx, "Failed to clean up incomplete tessdata at {Destination}", destination);
                }
            }
        }

        var rawPath = Path.Combine(localTessdataFolder, "eng.traineddata");
        if (_fileSystem.FileExists(rawPath))
        {
            try
            {
                _fileSystem.CopyFile(rawPath, destination, overwrite: true);
                _logger.LogInformation("Copied tessdata from {Source}", rawPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to copy tessdata from {Source}", rawPath);
            }
        }

        return false;
    }

    private async Task<bool> TryExtractFromResourceAsync(string destination, CancellationToken cancellationToken)
    {
        await using var resourceStream = AssetCatalog.OpenAssetStream("tessdata.eng.traineddata");
        if (resourceStream is null)
        {
            return false;
        }

        using var buffer = new MemoryStream();
        await resourceStream.CopyToAsync(buffer, cancellationToken);
        await _fileSystem.WriteAllBytesAsync(destination, buffer.ToArray(), cancellationToken);
        _logger.LogInformation("Provisioned tessdata resource to {Path}", destination);
        return true;
    }

    private void WarnIfPlaceholder(string tessdataPath)
    {
        try
        {
            var info = new FileInfo(tessdataPath);
            if (!info.Exists)
            {
                return;
            }

            if (info.Length < MinimumExpectedTessdataSizeBytes)
            {
                _logger.LogWarning("The tessdata file at {Path} is {Size} bytes. Replace it with the full language pack for production-grade OCR accuracy.", tessdataPath, info.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to inspect tessdata file at {Path}", tessdataPath);
        }
    }
}
