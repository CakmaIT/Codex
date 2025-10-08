using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Ozge.Core.Contracts;
using Ozge.Core.Domain.Enums;
using Ozge.Core.Models;
using Tesseract;
using UglyToad.PdfPig;
using PdfPigPage = UglyToad.PdfPig.Content.Page;

namespace Ozge.Ocr;

public sealed class OcrPipeline : IOcrPipeline
{
    private static readonly Regex WordRegex = new(@"[A-Za-z]{3,}", RegexOptions.Compiled);

    private readonly ILogger<OcrPipeline> _logger;
    private readonly IAssetProvisioner _assetProvisioner;
    private readonly IClock _clock;

    public OcrPipeline(
        ILogger<OcrPipeline> logger,
        IAssetProvisioner assetProvisioner,
        IClock clock)
    {
        _logger = logger;
        _assetProvisioner = assetProvisioner;
        _clock = clock;
    }

    public async Task<OcrParseResult> ParseAsync(ContentImportRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return request.ContentType switch
            {
                ContentType.Pdf => await ParsePdfAsync(request, cancellationToken),
                ContentType.Image => await ParseImageAsync(request, cancellationToken),
                ContentType.PlainText => await ParsePlainTextAsync(request, cancellationToken),
                _ => OcrParseResult.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse content from {Path}", request.SourcePath);
            return new OcrParseResult(
                ImmutableList<ExtractedUnit>.Empty,
                ImmutableList.Create(new ParseDiagnostic(
                    "OCR_ERROR",
                    $"Failed to parse content: {ex.Message}",
                    ParseDiagnosticSeverity.Error)));
        }
    }

    private Task<OcrParseResult> ParsePlainTextAsync(ContentImportRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var text = File.ReadAllText(request.SourcePath);
        return Task.FromResult(BuildResultFromText(text, request.DisplayName));
    }

    private Task<OcrParseResult> ParsePdfAsync(ContentImportRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var document = PdfDocument.Open(request.SourcePath);
        var textBuilder = new System.Text.StringBuilder();
        foreach (PdfPigPage page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            textBuilder.AppendLine(NormalizePageText(page));
        }

        var text = textBuilder.ToString();
        var result = BuildResultFromText(text, request.DisplayName);
        return Task.FromResult(result);
    }

    private async Task<OcrParseResult> ParseImageAsync(ContentImportRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _assetProvisioner.EnsureAssetsAsync(cancellationToken);

        var tessDataPath = _assetProvisioner.TessDataDirectory;

        using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
        using var pix = Pix.LoadFromFile(request.SourcePath);
        using var page = engine.Process(pix);
        var text = page.GetText();

        return BuildResultFromText(text, request.DisplayName);
    }

    private static string NormalizePageText(PdfPigPage page)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var word in page.GetWords())
        {
            builder.Append(word.Text);
            builder.Append(' ');
        }
        return builder.ToString();
    }

    private OcrParseResult BuildResultFromText(string text, string name)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new OcrParseResult(
                ImmutableList<ExtractedUnit>.Empty,
                ImmutableList.Create(new ParseDiagnostic(
                    "EMPTY",
                    "No readable text was detected.",
                    ParseDiagnosticSeverity.Warning)));
        }

        var words = WordRegex
            .Matches(text)
            .Select(match => match.Value.ToLowerInvariant())
            .GroupBy(value => value)
            .OrderByDescending(group => group.Count())
            .Take(32)
            .Select(group => CreateWord(group.Key))
            .ToImmutableList();

        var questions = words
            .Take(5)
            .Select(word => new GeneratedQuestion(
                QuestionType.MultipleChoice,
                $"Select the correct definition for '{word.Text}'.",
                word.Text,
                new[] { word.Text, $"{word.Text}?", $"{word.Text}!", $"{word.Text}..." },
                word.Difficulty))
            .ToImmutableList();

        var unit = new ExtractedUnit(
            Name: name,
            Topic: "Auto-import",
            Difficulty: DifficultyLevel.Medium,
            Words: words,
            Questions: questions);

        return new OcrParseResult(
            ImmutableList.Create(unit),
            ImmutableList.Create(new ParseDiagnostic(
                "GENERATED",
                $"Extracted {words.Count} words at {_clock.UtcNow}.",
                ParseDiagnosticSeverity.Info)));
    }

    private static ExtractedWord CreateWord(string text)
    {
        var difficulty = text.Length switch
        {
            <= 5 => DifficultyLevel.Easy,
            <= 8 => DifficultyLevel.Medium,
            _ => DifficultyLevel.Hard
        };

        return new ExtractedWord(
            Text: text,
            PartOfSpeech: PartOfSpeech.Unknown,
            Difficulty: difficulty,
            Definition: null,
            Synonym: null);
    }
}

