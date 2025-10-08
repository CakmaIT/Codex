using Microsoft.EntityFrameworkCore;
using Ozge.Core;
using Ozge.Core.Contracts;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Domain.Enums;

namespace Ozge.Data.Seeding;

public sealed class DataSeeder
{
    private static readonly string[] DefaultClassNames = { "5A", "5B", "5C", "5D" };
    private static readonly string[] DefaultGroupNames = { "Group A", "Group B", "Group C", "Group D", "Group E", "Group F", "Group G", "Group H" };

    private readonly OzgeDbContext _dbContext;
    private readonly IClock _clock;

    public DataSeeder(OzgeDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await EnsureClassesAsync(cancellationToken);
    }

    private async Task EnsureClassesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Classes.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var className in DefaultClassNames)
        {
            var classEntity = new ClassEntity
            {
                Name = className,
                SettingsJson = "{}"
            };

            foreach (var (groupName, index) in DefaultGroupNames.Select((name, index) => (name, index)))
            {
                classEntity.Groups.Add(new GroupEntity
                {
                    Name = $"{groupName}",
                    Avatar = $"avatar_{index + 1}",
                    Score = 0,
                    MetaJson = "{}"
                });
            }

            // Seed demo students
            for (var i = 1; i <= 12; i++)
            {
                classEntity.Students.Add(new StudentEntity
                {
                    Name = $"{className} Student {i:00}",
                    Seat = $"S{i:00}",
                    IsActive = true
                });
            }

            // Seed a demo unit with words/questions
            var unit = new UnitEntity
            {
                Name = "Unit 1",
                Topic = "Introductions",
                Difficulty = DifficultyLevel.Easy,
                MetaJson = "{}"
            };

            unit.Words.Add(new WordEntity
            {
                Text = "hello",
                PartOfSpeech = PartOfSpeech.Phrase,
                Difficulty = DifficultyLevel.Easy,
                MetaJson = "{}"
            });
            unit.Words.Add(new WordEntity
            {
                Text = "friend",
                PartOfSpeech = PartOfSpeech.Noun,
                Difficulty = DifficultyLevel.Easy,
                MetaJson = "{}"
            });
            unit.Questions.Add(new QuestionEntity
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What is a friendly greeting?",
                CorrectAnswer = "Hello!",
                OptionsJson = """["Hello!","Good night","Goodbye","See you"]""",
                Difficulty = DifficultyLevel.Easy
            });
            unit.Questions.Add(new QuestionEntity
            {
                Type = QuestionType.Speak,
                Prompt = "Introduce yourself using 'friend'.",
                CorrectAnswer = "Hello, this is my friend.",
                OptionsJson = "[]",
                Difficulty = DifficultyLevel.Medium
            });

            classEntity.Units.Add(unit);

            var session = new SessionEntity
            {
                Mode = LessonMode.Quiz,
                StartedAt = _clock.UtcNow.AddDays(-1),
                EndedAt = _clock.UtcNow.AddDays(-1).AddMinutes(45),
                Unit = unit
            };
            session.LessonLogs.Add(new LessonLogEntity
            {
                Class = classEntity,
                DataJson = """{"summary":"Sample lesson log"}""",
                Timestamp = _clock.UtcNow.AddDays(-1).AddMinutes(45)
            });

            classEntity.Sessions.Add(session);

            _dbContext.Classes.Add(classEntity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
