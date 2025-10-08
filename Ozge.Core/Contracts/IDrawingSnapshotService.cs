namespace Ozge.Core.Contracts;

public interface IDrawingSnapshotService
{
    Task<string> SaveSnapshotAsync(Guid classId, Guid unitId, Stream bitmapStream, CancellationToken cancellationToken);
    Task<Stream> OpenSnapshotAsync(string path, CancellationToken cancellationToken);
    Task<IEnumerable<string>> ListSnapshotsAsync(Guid classId, CancellationToken cancellationToken);
}
