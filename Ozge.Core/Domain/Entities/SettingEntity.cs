namespace Ozge.Core.Domain.Entities;

public class SettingEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public ClassEntity? Class { get; set; }
}
