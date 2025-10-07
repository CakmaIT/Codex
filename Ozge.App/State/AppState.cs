using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Ozge.Core.Models;

namespace Ozge.App.State;

public sealed record ClassState(ClassProfile Profile, ImmutableList<Group> Groups, ImmutableList<Unit> Units);

public sealed class AppState : ObservableObject
{
    private Guid _activeClassId;
    private string _activeMode = "HOME";
    private Guid? _activeUnitId;

    public Guid ActiveClassId
    {
        get => _activeClassId;
        set => SetProperty(ref _activeClassId, value);
    }

    public string ActiveMode
    {
        get => _activeMode;
        set => SetProperty(ref _activeMode, value);
    }

    public Guid? ActiveUnitId
    {
        get => _activeUnitId;
        set => SetProperty(ref _activeUnitId, value);
    }

    public ImmutableDictionary<Guid, ClassState> Classes { get; private set; } = ImmutableDictionary<Guid, ClassState>.Empty;

    public void SetClasses(IEnumerable<ClassState> classes)
    {
        Classes = classes.ToImmutableDictionary(x => x.Profile.Id);
        OnPropertyChanged(nameof(Classes));
    }
}
