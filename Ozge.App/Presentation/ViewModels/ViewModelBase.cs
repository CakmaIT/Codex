using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Ozge.App.Presentation.ViewModels;

public abstract partial class ViewModelBase : ObservableRecipient
{
    protected ViewModelBase(IMessenger messenger)
        : base(messenger)
    {
        IsActive = true;
    }
}
