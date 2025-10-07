using Ozge.Ocr.Parsing;

namespace Ozge.Ocr.Services;

public interface IContentParser
{
    Task<ParsedContent> ParseAsync(ContentSource source, CancellationToken cancellationToken = default);
}
