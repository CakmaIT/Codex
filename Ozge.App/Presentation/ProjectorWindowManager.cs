using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Ozge.App.Presentation.Windows;
using Ozge.Core.Contracts;

namespace Ozge.App.Presentation;

public sealed class ProjectorWindowManager : IProjectorWindowManager
{
    private const double DefaultWindowWidth = 1280;
    private const double DefaultWindowHeight = 720;

    private readonly ProjectorWindow _projectorWindow;
    private readonly IDisplayService _displayService;

    private bool _isVisible;
    private bool _isFullScreen;
    private string? _targetDisplayId;

    public ProjectorWindowManager(
        ProjectorWindow projectorWindow,
        IDisplayService displayService)
    {
        _projectorWindow = projectorWindow;
        _displayService = displayService;
        _targetDisplayId = displayService.DefaultProjectorDisplayId;
        _projectorWindow.Closed += (_, _) =>
        {
            _isVisible = false;
            _projectorWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            _projectorWindow.ResizeMode = ResizeMode.CanResize;
            _projectorWindow.Topmost = false;
        };
    }

    public bool IsProjectorVisible => _isVisible;
    public bool IsFullScreen => _isFullScreen;
    public string? CurrentDisplayId => _targetDisplayId;

    public void EnsureProjectorWindow()
    {
        if (!_projectorWindow.IsVisible)
        {
            ApplyWindowPlacement();
            _projectorWindow.Show();
            _isVisible = true;
        }
        else
        {
            ApplyWindowPlacement();
            _projectorWindow.Activate();
        }

        _projectorWindow.Focus();
    }

    public void ToggleFreeze(bool isFrozen)
    {
        if (!_isVisible)
        {
            return;
        }

        _projectorWindow.SetFreezeOverlay(isFrozen);
    }

    public void RevealAnswers(bool reveal)
    {
        if (!_isVisible)
        {
            return;
        }

        _projectorWindow.SetAnswerReveal(reveal);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        if (_isFullScreen == isFullScreen)
        {
            return;
        }

        _isFullScreen = isFullScreen;
        if (_isVisible)
        {
            ApplyWindowPlacement();
        }
    }

    public void SetTargetDisplay(string? displayId)
    {
        var normalized = string.IsNullOrWhiteSpace(displayId) ? null : displayId;
        _targetDisplayId = normalized;

        if (_isVisible)
        {
            ApplyWindowPlacement();
        }
    }

    private void ApplyWindowPlacement()
    {
        var screen = GetTargetScreen();

        if (screen is null)
        {
            _projectorWindow.WindowState = WindowState.Maximized;
            return;
        }

        _projectorWindow.WindowStartupLocation = WindowStartupLocation.Manual;

        if (_isFullScreen)
        {
            _projectorWindow.Topmost = true;
            _projectorWindow.WindowStyle = WindowStyle.None;
            _projectorWindow.ResizeMode = ResizeMode.NoResize;
            _projectorWindow.Left = screen.Bounds.Left;
            _projectorWindow.Top = screen.Bounds.Top;
            _projectorWindow.Width = screen.Bounds.Width;
            _projectorWindow.Height = screen.Bounds.Height;
            _projectorWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            _projectorWindow.Topmost = false;
            _projectorWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            _projectorWindow.ResizeMode = ResizeMode.CanResize;
            _projectorWindow.WindowState = WindowState.Normal;

            var width = Math.Min(DefaultWindowWidth, screen.Bounds.Width);
            var height = Math.Min(DefaultWindowHeight, screen.Bounds.Height);

            _projectorWindow.Width = width;
            _projectorWindow.Height = height;
            _projectorWindow.Left = screen.Bounds.Left + (screen.Bounds.Width - width) / 2;
            _projectorWindow.Top = screen.Bounds.Top + (screen.Bounds.Height - height) / 2;
        }
    }

    private Screen? GetTargetScreen()
    {
        if (!string.IsNullOrWhiteSpace(_targetDisplayId))
        {
            var matching = Screen.AllScreens.FirstOrDefault(s =>
                string.Equals(s.DeviceName, _targetDisplayId, StringComparison.OrdinalIgnoreCase));
            if (matching is not null)
            {
                return matching;
            }
        }

        var defaultId = _displayService.DefaultProjectorDisplayId;
        if (!string.IsNullOrWhiteSpace(defaultId))
        {
            var defaultScreen = Screen.AllScreens.FirstOrDefault(s =>
                string.Equals(s.DeviceName, defaultId, StringComparison.OrdinalIgnoreCase));
            if (defaultScreen is not null)
            {
                _targetDisplayId = defaultId;
                return defaultScreen;
            }
        }

        var fallback = Screen.AllScreens.FirstOrDefault();
        _targetDisplayId = fallback?.DeviceName;
        return fallback;
    }
}
