using System;
using System.Collections.Immutable;
using System.Linq;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Domain.Enums;
using Ozge.Core.State;

namespace Ozge.App.Infrastructure;

internal static class ClassStateMapper
{
    public static ClassState Map(ClassEntity entity)
    {
        var groups = entity.Groups
            .Select(group => new GroupState(
                group.Id,
                group.Name,
                group.Avatar,
                group.Score,
                0,
                false,
                DateTimeOffset.UtcNow))
            .ToImmutableList();

        var units = entity.Units
            .Select(unit => new UnitSummary(
                unit.Id,
                unit.Name,
                unit.Topic,
                unit.Difficulty,
                unit.Words.Count,
                unit.Questions.Count,
                false))
            .ToImmutableList();

        var students = entity.Students
            .Select(student => new StudentState(
                student.Id,
                student.Name,
                student.Seat,
                true,
                student.IsActive))
            .ToImmutableList();

        var lessonLogs = entity.LessonLogs
            .Select(log => new LessonLogSnapshot(
                log.SessionId,
                LessonMode.Quiz,
                null,
                log.Timestamp,
                log.Timestamp,
                "Demo lesson"))
            .ToImmutableList();

        return new ClassState(
            entity.Id,
            entity.Name,
            groups,
            units,
            students,
            ImmutableList<ScoreSnapshot>.Empty,
            ImmutableList<BehaviorSnapshot>.Empty,
            lessonLogs);
    }
}
