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

    public ProjectorWindowManager(
        ProjectorWindow projectorWindow,
        IDisplayService displayService)
    {
        _projectorWindow = projectorWindow;
        _displayService = displayService;
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
        var displayId = _displayService.DefaultProjectorDisplayId;

        if (displayId is not null)
        {
            var matching = Screen.AllScreens.FirstOrDefault(s =>
                string.Equals(s.DeviceName, displayId, StringComparison.OrdinalIgnoreCase));
            if (matching is not null)
            {
                return matching;
            }
        }

        return Screen.AllScreens.FirstOrDefault();
    }
}
