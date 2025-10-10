using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface IDisplayService
{
    IReadOnlyList<DisplayDescriptor> GetDisplays();
    DisplayDescriptor? GetDisplay(string id);
    string? DefaultProjectorDisplayId { get; }
}
