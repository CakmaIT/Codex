using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ozge.Core.Messaging;

namespace Ozge.App.ViewModels;

public partial class ProjectorViewModel : ObservableRecipient,
    IRecipient<ModeChangedMessage>,
    IRecipient<RevealAnswerMessage>,
    IRecipient<ToastMessage>
{
    [ObservableProperty]
    private string _mode = "HOME";

    [ObservableProperty]
    private string _prompt = "Welcome";

    [ObservableProperty]
    private string _feedback = string.Empty;

    private bool _isReveal;

    public ProjectorViewModel(IMessenger messenger) : base(messenger)
    {
        IsActive = true;
    }

    public void Receive(ModeChangedMessage message)
    {
        Mode = message.Mode;
        Prompt = message.UnitId.HasValue ? $"Unit: {message.UnitId}" : "";
        Feedback = string.Empty;
    }

    public void Receive(RevealAnswerMessage message)
    {
        _isReveal = message.IsRevealed;
        Feedback = _isReveal ? "Answers revealed" : "Awaiting teacher";
    }

    public void Receive(ToastMessage message)
    {
        if (_isReveal)
        {
            Feedback = message.Value;
        }
    }
}
