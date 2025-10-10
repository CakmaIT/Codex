namespace Ozge.App.Presentation;

public interface IProjectorWindowManager
{
    void EnsureProjectorWindow();
    void ToggleFreeze(bool isFrozen);
    void RevealAnswers(bool reveal);
    bool IsProjectorVisible { get; }
    bool IsFullScreen { get; }
    void SetFullScreen(bool isFullScreen);
    void SetTargetDisplay(string? displayId);
    string? CurrentDisplayId { get; }
}
