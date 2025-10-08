namespace Ozge.Core.Models;

public sealed record QuizQuestionData(
    Guid QuestionId,
    string Prompt,
    IReadOnlyList<string> Options,
    string CorrectAnswer);
