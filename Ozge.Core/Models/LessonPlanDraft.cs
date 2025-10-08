using System.Collections.Immutable;

namespace Ozge.Core.Models;

public sealed record LessonPlanDraft(
    string Summary,
    ImmutableList<ExtractedWord> WordList,
    ImmutableList<GeneratedQuestion> Quiz,
    ImmutableList<string> SpeakingPrompts,
    ImmutableList<string> StoryPrompts)
{
    public static LessonPlanDraft Empty { get; } = new(
        string.Empty,
        ImmutableList<ExtractedWord>.Empty,
        ImmutableList<GeneratedQuestion>.Empty,
        ImmutableList<string>.Empty,
        ImmutableList<string>.Empty);
}
