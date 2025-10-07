using System.Collections.Generic;

namespace Ozge.Core.Domain.Entities;

public class UnitEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string MetaJson { get; set; } = "{}";
    public ClassEntity? Class { get; set; }
    public ICollection<WordEntity> Words { get; set; } = new List<WordEntity>();
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
}
