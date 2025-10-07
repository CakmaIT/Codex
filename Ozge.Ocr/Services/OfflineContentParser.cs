using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ozge.Ocr.Parsing;
using UglyToad.PdfPig;

namespace Ozge.Ocr.Services;

public class OfflineContentParser : IContentParser
{
    public Task<ParsedContent> ParseAsync(ContentSource source, CancellationToken cancellationToken = default)
    {
        if (source.Path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ParsePdf(source.Path));
        }

        return Task.FromResult(new ParsedContent(Array.Empty<ParsedUnit>(), new[] { "No parser available for source." }));
    }

    private ParsedContent ParsePdf(string path)
    {
        var diagnostics = new List<string>();
        var units = new List<ParsedUnit>();

        if (!File.Exists(path))
        {
            diagnostics.Add($"File not found: {path}");
            return new ParsedContent(units, diagnostics);
        }

        using var document = PdfDocument.Open(path);
        var text = string.Join(Environment.NewLine, document.GetPages().Select(p => p.Text));
        var words = text.Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Take(50)
            .ToList();

        var unit = new ParsedUnit("Imported Unit", "General", words, words.Take(10).ToList(), words.Take(5).ToList());
        units.Add(unit);
        diagnostics.Add($"Parsed {words.Count} words from PDF.");
        return new ParsedContent(units, diagnostics);
    }
}
