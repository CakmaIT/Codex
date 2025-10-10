using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Domain.Entities;

public class QuestionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UnitId { get; set; }
    public QuestionType Type { get; set; } = QuestionType.Unknown;
    public string Prompt { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = "[]";
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    public UnitEntity? Unit { get; set; }
}
