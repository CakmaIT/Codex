using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using Ozge.Core.Contracts;
using Ozge.Core.Models;

namespace Ozge.App.Infrastructure;

public sealed class DisplayService : IDisplayService
{
    private ImmutableList<DisplayDescriptor> _cache = ImmutableList<DisplayDescriptor>.Empty;

    public IReadOnlyList<DisplayDescriptor> GetDisplays()
    {
        if (_cache.Any())
        {
            return _cache;
        }

        var displays = Screen.AllScreens
            .Select(screen => new DisplayDescriptor(
                Id: screen.DeviceName,
                FriendlyName: screen.DeviceName,
                IsPrimary: screen.Primary,
                Width: screen.Bounds.Width,
                Height: screen.Bounds.Height))
            .ToImmutableList();

        _cache = displays;
        return displays;
    }

    public DisplayDescriptor? GetDisplay(string id)
    {
        return GetDisplays().FirstOrDefault(display => string.Equals(display.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public string? DefaultProjectorDisplayId
        => GetDisplays().FirstOrDefault(display => !display.IsPrimary)?.Id;
}
