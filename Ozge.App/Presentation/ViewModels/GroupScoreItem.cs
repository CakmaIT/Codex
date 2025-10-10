using System;
using Ozge.Core.State;

namespace Ozge.App.Presentation.ViewModels;

public sealed class GroupScoreItem
{
    public Guid Id { get; }
    public string Name { get; }
    public int Score { get; }

    public GroupScoreItem(Guid id, string name, int score)
    {
        Id = id;
        Name = name;
        Score = score;
    }

    public static GroupScoreItem FromState(GroupState state) => new(state.Id, state.Name, state.Score);
}
