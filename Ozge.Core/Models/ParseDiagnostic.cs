namespace Ozge.Core.Models;

public sealed record ParseDiagnostic(
    string Code,
    string Message,
    ParseDiagnosticSeverity Severity);
