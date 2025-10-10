namespace Ozge.Core.State;

public sealed record StudentState(
    Guid Id,
    string Name,
    string Seat,
    bool IsPresent,
    bool IsActive);
