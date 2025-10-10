namespace Ozge.Core.Contracts;

public interface IBackupService
{
    Task RunSnapshotAsync(CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetRecentBackupsAsync(CancellationToken cancellationToken);
    Task RestoreAsync(string backupPath, CancellationToken cancellationToken);
}
