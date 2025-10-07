namespace Ozge.Core.Domain.Entities;

public class WordEntity
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "easy";
    public string MetaJson { get; set; } = "{}";
    public UnitEntity? Unit { get; set; }
}
