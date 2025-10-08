using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ozge.Core.Models;
using Ozge.Data.Context;

namespace Ozge.Data.Seed;

public class DataSeeder
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<DataSeeder> _logger;

    private static readonly string[] DefaultClasses = ["5A", "5B", "5C", "5D"];
    private static readonly string[] DefaultGroups = ["Group A", "Group B", "Group C", "Group D", "Group E", "Group F", "Group G", "Group H"];

    public DataSeeder(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<DataSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        if (await context.Classes.AnyAsync(cancellationToken))
        {
            return;
        }

        var random = new Random(42);

        foreach (var className in DefaultClasses)
        {
            var classId = Guid.NewGuid();
            context.Classes.Add(new ClassProfile
            {
                Id = classId,
                Name = className,
                SettingsJson = "{\"pin\":\"1234\"}"
            });

            foreach (var (groupName, index) in DefaultGroups.Select((name, index) => (name, index)))
            {
                context.Groups.Add(new Group
                {
                    Id = Guid.NewGuid(),
                    ClassId = classId,
                    Name = groupName,
                    Avatar = $"avatar_{index % 4}",
                    Score = 0
                });
            }

            for (var studentIndex = 0; studentIndex < 24; studentIndex++)
            {
                context.Students.Add(new Student
                {
                    Id = Guid.NewGuid(),
                    ClassId = classId,
                    Name = $"Student {studentIndex + 1}",
                    Seat = ((char)('A' + (studentIndex % 6))).ToString() + (studentIndex / 6 + 1),
                    IsActive = true
                });
            }

            for (var unitIndex = 1; unitIndex <= 5; unitIndex++)
            {
                var unitId = Guid.NewGuid();
                context.Units.Add(new Unit
                {
                    Id = unitId,
                    ClassId = classId,
                    Name = $"Unit {unitIndex}",
                    Topic = $"Topic {unitIndex}",
                    MetaJson = "{}"
                });

                for (var wordIndex = 0; wordIndex < 30; wordIndex++)
                {
                    var word = new Word
                    {
                        Id = Guid.NewGuid(),
                        UnitId = unitId,
                        Text = $"Word{unitIndex}_{wordIndex}",
                        PartOfSpeech = wordIndex % 3 switch
                        {
                            0 => "noun",
                            1 => "verb",
                            _ => "adjective"
                        },
                        Difficulty = random.Next(0, 3) switch
                        {
                            0 => "easy",
                            1 => "medium",
                            _ => "hard"
                        },
                        MetaJson = "{}"
                    };
                    context.Words.Add(word);
                }

                for (var questionIndex = 0; questionIndex < 20; questionIndex++)
                {
                    var correct = $"Word{unitIndex}_{questionIndex % 30}";
                    var options = Enumerable.Range(0, 4).Select(i => $"Option {i + 1}").ToArray();
                    context.Questions.Add(new Question
                    {
                        Id = Guid.NewGuid(),
                        UnitId = unitId,
                        Type = "MCQ",
                        Prompt = $"What is the meaning of {correct}?",
                        Correct = correct,
                        OptionsJson = System.Text.Json.JsonSerializer.Serialize(options),
                        Difficulty = random.Next(0, 3) switch
                        {
                            0 => "easy",
                            1 => "medium",
                            _ => "hard"
                        }
                    });
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded default data for Ozge2");
    }
}
