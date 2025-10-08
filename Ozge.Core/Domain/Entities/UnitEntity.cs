using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Domain.Entities;

public class UnitEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string MetaJson { get; set; } = "{}";
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    public ClassEntity? Class { get; set; }
    public ICollection<WordEntity> Words { get; set; } = new List<WordEntity>();
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    public ICollection<SnapshotEntity> Snapshots { get; set; } = new List<SnapshotEntity>();
}
