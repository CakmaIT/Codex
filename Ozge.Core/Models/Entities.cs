namespace Ozge.Core.Models;

public class ClassProfile
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string SettingsJson { get; set; } = "{}";
}

public class Student
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public required string Name { get; set; }
    public string? Seat { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Group
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public required string Name { get; set; }
    public string Avatar { get; set; } = "default";
    public int Score { get; set; }
}

public class Unit
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public required string Name { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string MetaJson { get; set; } = "{}";
}

public class Word
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public required string Text { get; set; }
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "medium";
    public string MetaJson { get; set; } = "{}";
}

public class Question
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public required string Type { get; set; }
    public required string Prompt { get; set; }
    public required string Correct { get; set; }
    public string OptionsJson { get; set; } = "[]";
    public string Difficulty { get; set; } = "medium";
}

public class Session
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Mode { get; set; } = "HOME";
    public Guid? UnitId { get; set; }
}

public class ScoreEvent
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid GroupId { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class BehaviorEvent
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid GroupId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Note { get; set; }
}

public class Snapshot
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid? UnitId { get; set; }
    public required string Path { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Setting
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
}

public class Attendance
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }
    public DateOnly Date { get; set; }
    public bool Present { get; set; }
}

public class LessonLog
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SessionId { get; set; }
    public string DataJson { get; set; } = "{}";
    public DateTime Timestamp { get; set; }
}

public class Job
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Error { get; set; }
}
