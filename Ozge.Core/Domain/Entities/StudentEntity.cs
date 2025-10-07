using System.Collections.Generic;

namespace Ozge.Core.Domain.Entities;

public class StudentEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Seat { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ClassEntity? Class { get; set; }
    public ICollection<AttendanceEntity> AttendanceRecords { get; set; } = new List<AttendanceEntity>();
}
