using System.Collections.Immutable;

namespace Ozge.Core.Models;

public sealed record QuestionImportResult(
    int QuestionsAdded,
    int WordsAdded,
    ImmutableList<ParseDiagnostic> Diagnostics);
