using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Models;

public sealed record ContentImportRequest(
    string SourcePath,
    string DisplayName,
    ContentType ContentType,
    string? PageRangeExpression);
