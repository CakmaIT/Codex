using System.IO;
using Microsoft.Extensions.Logging;
using Ozge.Assets;
using Ozge.Core;
using Ozge.Core.Contracts;

namespace Ozge.App.Infrastructure;

public sealed class AssetProvisioner : IAssetProvisioner
{
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
        if (_fileSystem.FileExists(tessdataTarget))
        {
            return;
        }

        await using var resourceStream = AssetCatalog.OpenAssetStream("tessdata.eng.traineddata");
        if (resourceStream is null)
        {
            _logger.LogWarning("Could not locate embeddable tessdata resource. OCR accuracy may be reduced.");
            return;
        }

        using var memory = new MemoryStream();
        await resourceStream.CopyToAsync(memory, cancellationToken);
        await _fileSystem.WriteAllBytesAsync(tessdataTarget, memory.ToArray(), cancellationToken);
        _logger.LogInformation("Provisioned tessdata resource to {Path}", tessdataTarget);
    }
}
