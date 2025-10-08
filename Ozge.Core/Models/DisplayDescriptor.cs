namespace Ozge.Core.Models;

public sealed record DisplayDescriptor(
    string Id,
    string FriendlyName,
    bool IsPrimary,
    double Width,
    double Height);
