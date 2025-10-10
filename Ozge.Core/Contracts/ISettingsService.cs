using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface ISettingsService
{
    Task<AppConfiguration> GetAsync(Guid classId, CancellationToken cancellationToken);
    Task SaveAsync(Guid classId, AppConfiguration configuration, CancellationToken cancellationToken);
}
