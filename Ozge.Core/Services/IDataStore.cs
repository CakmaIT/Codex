using Ozge.Core.Models;

namespace Ozge.Core.Services;

public interface IDataStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassProfile>> GetClassesAsync(CancellationToken cancellationToken = default);
    Task<ClassProfile> GetClassAsync(Guid classId, CancellationToken cancellationToken = default);
    Task SaveClassAsync(ClassProfile profile, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetGroupsAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unit>> GetUnitsAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Word>> GetWordsAsync(Guid unitId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetQuestionsAsync(Guid unitId, CancellationToken cancellationToken = default);
    Task AppendScoreEventAsync(ScoreEvent scoreEvent, CancellationToken cancellationToken = default);
    Task AppendBehaviorEventAsync(BehaviorEvent behaviorEvent, CancellationToken cancellationToken = default);
    Task AppendLessonLogAsync(LessonLog log, CancellationToken cancellationToken = default);
}
