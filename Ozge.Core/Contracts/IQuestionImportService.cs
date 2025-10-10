using System;
using System.Threading;
using System.Threading.Tasks;
using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface IQuestionImportService
{
    Task<QuestionImportResult> ImportPdfAsync(Guid classId, Guid unitId, string filePath, CancellationToken cancellationToken);
}
