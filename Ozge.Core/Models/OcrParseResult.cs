using System.Collections.Immutable;

namespace Ozge.Core.Models;

public sealed record OcrParseResult(
    ImmutableList<ExtractedUnit> Units,
    ImmutableList<ParseDiagnostic> Diagnostics)
{
    public static OcrParseResult Empty { get; } = new(
        ImmutableList<ExtractedUnit>.Empty,
        ImmutableList<ParseDiagnostic>.Empty);
}
