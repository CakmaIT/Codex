using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Services;
using Ozge.Data.Context;
using Ozge.Data.Extensions;

namespace Ozge.Data.Services;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly OzgeDbContext _dbContext;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(OzgeDbContext dbContext, ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ozge2"));

        if (_dbContext.Database.GetDbConnection() is SqliteConnection sqliteConnection)
        {
            sqliteConnection.Open();
            sqliteConnection.ApplyRecommendedPragmas();
        }

        await _dbContext.Database.MigrateAsync(cancellationToken);
        await SeedAsync(cancellationToken);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Classes.AnyAsync(cancellationToken))
        {
            return;
        }

        var classNames = new[] { "5A", "5B", "5C", "5D" };

        foreach (var className in classNames)
        {
            var classEntity = new ClassEntity
            {
                Id = Guid.NewGuid(),
                Name = className,
                SettingsJson = "{}"
            };

            var groups = Enumerable.Range(0, 8).Select(i => new GroupEntity
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                Name = ((char)('A' + i)).ToString(),
                Avatar = $"avatar_{i}",
                Score = 0
            }).ToList();

            var students = Enumerable.Range(1, 24).Select(i => new StudentEntity
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                Name = $"Student {i}",
                Seat = i.ToString("D2"),
                IsActive = true
            }).ToList();

            var units = Enumerable.Range(1, 5).Select(u => new UnitEntity
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                Name = $"Unit {u}",
                Topic = $"Topic {u}",
                MetaJson = "{}"
            }).ToList();

            var words = new List<WordEntity>();
            var questions = new List<QuestionEntity>();

            foreach (var unit in units)
            {
                var unitWords = Enumerable.Range(1, 20).Select(idx => new WordEntity
                {
                    Id = Guid.NewGuid(),
                    UnitId = unit.Id,
                    Text = $"Word {idx}",
                    PartOfSpeech = idx % 2 == 0 ? "noun" : "verb",
                    Difficulty = idx % 3 == 0 ? "medium" : "easy",
                    MetaJson = "{}"
                }).ToList();

                words.AddRange(unitWords);

                questions.AddRange(unitWords.Take(5).Select(word => new QuestionEntity
                {
                    Id = Guid.NewGuid(),
                    UnitId = unit.Id,
                    Type = "Quiz",
                    Prompt = $"Select the definition of {word.Text}",
                    Correct = word.Text,
                    OptionsJson = "[]",
                    Difficulty = word.Difficulty
                }));
            }

            _dbContext.Classes.Add(classEntity);
            _dbContext.Groups.AddRange(groups);
            _dbContext.Students.AddRange(students);
            _dbContext.Units.AddRange(units);
            _dbContext.Words.AddRange(words);
            _dbContext.Questions.AddRange(questions);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
