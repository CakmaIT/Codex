using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface IAnalyticsService
{
    Task<AnalyticsSnapshot> GetWeeklyAnalyticsAsync(Guid classId, CancellationToken cancellationToken);
}
