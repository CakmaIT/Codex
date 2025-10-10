using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface IQuizSessionService
{
    Task<QuizSessionData> LoadQuizAsync(Guid classId, Guid unitId, CancellationToken cancellationToken);
}
