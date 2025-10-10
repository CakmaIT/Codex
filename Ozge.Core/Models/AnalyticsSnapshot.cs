using System.Collections.Immutable;
using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Models;

public sealed record AnalyticsSnapshot(
    ImmutableDictionary<LessonMode, double> AccuracyByMode,
    ImmutableDictionary<Guid, double> AccuracyByUnit,
    ImmutableDictionary<string, int> MostMissedWords,
    ImmutableDictionary<LessonMode, TimeSpan> AverageTimePerQuestion,
    ImmutableDictionary<Guid, int> BehaviorCounts);
