using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface IContentPackService
{
    Task SavePackAsync(Guid classId, string destinationPath, IEnumerable<ExtractedUnit> units, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExtractedUnit>> LoadPackAsync(string sourcePath, CancellationToken cancellationToken);
}
