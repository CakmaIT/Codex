using Microsoft.Extensions.Logging;
using Ozge.Core.Models;

namespace Ozge.Ocr.Services;

public class OfflineContentIngestionService : IContentIngestionService
{
    private readonly ILogger<OfflineContentIngestionService> _logger;

    public OfflineContentIngestionService(ILogger<OfflineContentIngestionService> logger)
    {
        _logger = logger;
    }

    public Task<ContentIngestionResult> ImportAsync(ContentIngestionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing content from {SourcePath} for class {ClassId}", request.SourcePath, request.ClassId);

        // Placeholder pipeline until OCR and PDF parsing are implemented.
        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            ClassId = request.ClassId,
            Name = "Imported Unit",
            Topic = "Auto",
            MetaJson = "{}"
        };

        var words = Enumerable.Range(1, 10).Select(i => new Word
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            Text = $"SampleWord{i}",
            PartOfSpeech = "noun",
            Difficulty = i % 3 == 0 ? "hard" : i % 2 == 0 ? "medium" : "easy",
            MetaJson = "{}"
        }).ToList();

        var questions = Enumerable.Range(1, 5).Select(i => new Question
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            Type = "MCQ",
            Prompt = $"Placeholder question {i}",
            Correct = words[i % words.Count].Text,
            OptionsJson = System.Text.Json.JsonSerializer.Serialize(words.Select(w => w.Text).Take(4)),
            Difficulty = "medium"
        }).ToList();

        return Task.FromResult(new ContentIngestionResult(new[] { unit }, words, questions));
    }
}
