using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Ozge.Core.Messaging;

public record ClassChangedMessage(Guid ClassId);
public record ModeChangedMessage(string Mode, Guid? UnitId);
public record RevealAnswerMessage(bool IsRevealed);

public sealed class ToastMessage : ValueChangedMessage<string>
{
    public ToastMessage(string value) : base(value)
    {
    }
}
