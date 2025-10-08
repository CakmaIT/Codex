using Ozge.Core.Models;

namespace Ozge.Ocr.Services;

public interface IContentIngestionService
{
    Task<ContentIngestionResult> ImportAsync(ContentIngestionRequest request, CancellationToken cancellationToken = default);
}

public record ContentIngestionRequest(Guid ClassId, string SourcePath, string[]? PageRanges = null);

public record ContentIngestionResult(IReadOnlyList<Unit> Units, IReadOnlyList<Word> Words, IReadOnlyList<Question> Questions);
