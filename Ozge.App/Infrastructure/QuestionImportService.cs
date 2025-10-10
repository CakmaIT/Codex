using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ozge.Core.Contracts;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Domain.Enums;
using Ozge.Core.Models;
using Ozge.Core.State;
using Ozge.Data;

namespace Ozge.App.Infrastructure;

public sealed class QuestionImportService : IQuestionImportService
{
    private readonly IOcrPipeline _ocrPipeline;
    private readonly IDbContextFactory<OzgeDbContext> _dbContextFactory;
    private readonly IAppStateStore _stateStore;
    private readonly ILogger<QuestionImportService> _logger;

    public QuestionImportService(
        IOcrPipeline ocrPipeline,
        IDbContextFactory<OzgeDbContext> dbContextFactory,
        IAppStateStore stateStore,
        ILogger<QuestionImportService> logger)
    {
        _ocrPipeline = ocrPipeline;
        _dbContextFactory = dbContextFactory;
        _stateStore = stateStore;
        _logger = logger;
    }

    public async Task<QuestionImportResult> ImportPdfAsync(Guid classId, Guid unitId, string filePath, CancellationToken cancellationToken)
    {
        if (classId == Guid.Empty)
        {
            throw new ArgumentException("ClassId must be provided.", nameof(classId));
        }

        if (unitId == Guid.Empty)
        {
            throw new ArgumentException("UnitId must be provided.", nameof(unitId));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must be provided.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Belirtilen PDF dosyasi bulunamadi.", filePath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var request = new ContentImportRequest(
            filePath,
            Path.GetFileNameWithoutExtension(filePath),
            ContentType.Pdf,
            null);

        var parseResult = await _ocrPipeline.ParseAsync(request, cancellationToken);
        if (!parseResult.Units.Any())
        {
            _logger.LogWarning("PDF import returned no units for {Path}", filePath);
            return new QuestionImportResult(0, 0, parseResult.Diagnostics);
        }

        var firstUnit = parseResult.Units.First();
        var generatedQuestions = firstUnit.Questions;

        if (generatedQuestions.Count == 0 && firstUnit.Words.Count == 0)
        {
            return new QuestionImportResult(0, 0, parseResult.Diagnostics);
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var unit = await db.Units
            .Include(u => u.Questions)
            .Include(u => u.Words)
            .FirstOrDefaultAsync(u => u.Id == unitId && u.ClassId == classId, cancellationToken);

        if (unit is null)
        {
            throw new InvalidOperationException("Secilen unite veritabaninda bulunamadi.");
        }

        var questionsAdded = 0;
        foreach (var question in generatedQuestions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var prompt = question.Prompt?.Trim();
            var correctAnswer = question.CorrectAnswer?.Trim();
            if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(correctAnswer))
            {
                continue;
            }

            var options = question.Options?
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Select(option => option.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (!options.Contains(correctAnswer, StringComparer.OrdinalIgnoreCase))
            {
                options.Insert(0, correctAnswer);
            }

            var entity = new QuestionEntity
            {
                UnitId = unitId,
                Type = question.Type == QuestionType.Unknown ? QuestionType.MultipleChoice : question.Type,
                Prompt = prompt,
                CorrectAnswer = correctAnswer,
                OptionsJson = JsonSerializer.Serialize(options),
                Difficulty = question.Difficulty
            };

            db.Questions.Add(entity);
            questionsAdded++;
        }

        var wordsAdded = 0;
        if (firstUnit.Words.Count > 0)
        {
            var existingWords = new HashSet<string>(unit.Words.Select(w => w.Text), StringComparer.OrdinalIgnoreCase);
            foreach (var word in firstUnit.Words)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(word.Text) || !existingWords.Add(word.Text))
                {
                    continue;
                }

                db.Words.Add(new WordEntity
                {
                    UnitId = unitId,
                    Text = word.Text,
                    PartOfSpeech = word.PartOfSpeech,
                    Difficulty = word.Difficulty,
                    MetaJson = "{}"
                });

                wordsAdded++;
            }
        }

        if (questionsAdded == 0 && wordsAdded == 0)
        {
            return new QuestionImportResult(0, 0, parseResult.Diagnostics);
        }

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Imported {QuestionCount} questions and {WordCount} words from {File}", questionsAdded, wordsAdded, filePath);

        await RefreshClassStateAsync(db, classId, unitId, cancellationToken);

        return new QuestionImportResult(questionsAdded, wordsAdded, parseResult.Diagnostics);
    }

    private async Task RefreshClassStateAsync(OzgeDbContext db, Guid classId, Guid unitId, CancellationToken cancellationToken)
    {
        var refreshedClass = await db.Classes
            .Where(c => c.Id == classId)
            .Include(c => c.Groups)
            .Include(c => c.Units).ThenInclude(u => u.Words)
            .Include(c => c.Units).ThenInclude(u => u.Questions)
            .Include(c => c.Students)
            .Include(c => c.LessonLogs)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (refreshedClass is null)
        {
            return;
        }

        var classState = ClassStateMapper.Map(refreshedClass);
        _stateStore.Update(builder =>
        {
            builder.UpdateClass(classState);
            builder.ActiveClassId = classId;
            builder.ActiveUnitId = unitId;
            return builder;
        });
    }
}
