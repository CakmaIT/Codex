using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Domain.Entities;

public class WordEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UnitId { get; set; }
    public string Text { get; set; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; set; } = PartOfSpeech.Unknown;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public string MetaJson { get; set; } = "{}";

    public UnitEntity? Unit { get; set; }
}
