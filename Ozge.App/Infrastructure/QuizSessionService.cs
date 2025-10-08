using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ozge.Core.Contracts;
using Ozge.Core.Models;
using Ozge.Data;

namespace Ozge.App.Infrastructure;

public sealed class QuizSessionService : IQuizSessionService
{
    private readonly IDbContextFactory<OzgeDbContext> _dbContextFactory;
    private readonly ILogger<QuizSessionService> _logger;

    public QuizSessionService(
        IDbContextFactory<OzgeDbContext> dbContextFactory,
        ILogger<QuizSessionService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<QuizSessionData> LoadQuizAsync(Guid classId, Guid unitId, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var questions = await context.Questions
            .AsNoTracking()
            .Where(q => q.UnitId == unitId)
            .OrderBy(q => q.Prompt)
            .Select(q => new
            {
                q.Id,
                q.Prompt,
                q.CorrectAnswer,
                q.OptionsJson
            })
            .ToListAsync(cancellationToken);

        var mapped = questions
            .Select(q =>
            {
                var options = ParseOptions(q.OptionsJson);
                if (!options.Contains(q.CorrectAnswer, StringComparer.OrdinalIgnoreCase))
                {
                    options.Add(q.CorrectAnswer);
                }

                return new QuizQuestionData(
                    q.Id,
                    q.Prompt,
                    options.ToArray(),
                    q.CorrectAnswer);
            })
            .ToList();

        _logger.LogInformation("Loaded {Count} quiz questions for unit {UnitId}", mapped.Count, unitId);

        return new QuizSessionData(classId, unitId, mapped);
    }

    private static List<string> ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
        {
            return new List<string>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(optionsJson);
            return parsed is null
                ? new List<string>()
                : parsed.Where(o => !string.IsNullOrWhiteSpace(o)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }
}
