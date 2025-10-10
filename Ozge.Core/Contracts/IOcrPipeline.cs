using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface IOcrPipeline
{
    Task<OcrParseResult> ParseAsync(ContentImportRequest request, CancellationToken cancellationToken);
}
