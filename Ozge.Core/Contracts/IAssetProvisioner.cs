namespace Ozge.Core.Contracts;

public interface IAssetProvisioner
{
    string TessDataDirectory { get; }
    Task EnsureAssetsAsync(CancellationToken cancellationToken);
}
