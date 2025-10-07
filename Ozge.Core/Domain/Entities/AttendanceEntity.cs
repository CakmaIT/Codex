namespace Ozge.Core.Domain.Entities;

public class AttendanceEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }
    public DateOnly Date { get; set; }
    public bool Present { get; set; }
    public StudentEntity? Student { get; set; }
    public ClassEntity? Class { get; set; }
}
