namespace Ozge.Core.Messaging;

public record ProjectorRevealChangedMessage(bool IsRevealed);
public record ActiveModeChangedMessage(string Mode);
public record ScoreUpdatedMessage(Guid ClassId, Guid GroupId, int NewScore);
public record BehaviorEventMessage(Guid ClassId, Guid GroupId, string Kind);
