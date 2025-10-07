namespace Ozge.Core.Domain.Entities;

public class QuestionEntity
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Correct { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = "[]";
    public string Difficulty { get; set; } = "easy";
    public UnitEntity? Unit { get; set; }
}
