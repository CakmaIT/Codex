using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Models;

public sealed record GeneratedQuestion(
    QuestionType Type,
    string Prompt,
    string CorrectAnswer,
    IReadOnlyList<string> Options,
    DifficultyLevel Difficulty);
