namespace Ozge.Core.Services;

public interface IScreenSelectionService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    int? ProjectorScreenIndex { get; }
    Task SetProjectorScreenIndexAsync(int index, CancellationToken cancellationToken = default);
}
