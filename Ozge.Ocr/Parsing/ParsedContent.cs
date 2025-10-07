namespace Ozge.Ocr.Parsing;

public record ParsedContent(
    IReadOnlyList<ParsedUnit> Units,
    IReadOnlyList<string> Diagnostics);

public record ParsedUnit(
    string Name,
    string Topic,
    IReadOnlyList<string> Words,
    IReadOnlyList<string> Sentences,
    IReadOnlyList<string> Questions);

public record ContentSource(string Path, byte[]? Data = null, string? ContentType = null);
