namespace Ozge.Core.Contracts;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
    DateOnly Today { get; }
}
