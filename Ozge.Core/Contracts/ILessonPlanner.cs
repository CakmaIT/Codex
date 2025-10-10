using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface ILessonPlanner
{
    Task<LessonPlanDraft> BuildLessonPlanAsync(Guid classId, Guid unitId, CancellationToken cancellationToken);
}
