using System.Collections.Generic;

namespace Ozge.Core.Domain.Entities;

public class GroupEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public int Score { get; set; }
    public ClassEntity? Class { get; set; }
    public ICollection<ScoreEventEntity> ScoreEvents { get; set; } = new List<ScoreEventEntity>();
    public ICollection<BehaviorEventEntity> BehaviorEvents { get; set; } = new List<BehaviorEventEntity>();
}
