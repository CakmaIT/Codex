namespace Ozge.Core.Domain.Entities;

public class AttendanceEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public bool Present { get; set; }

    public ClassEntity? Class { get; set; }
    public StudentEntity? Student { get; set; }
}
