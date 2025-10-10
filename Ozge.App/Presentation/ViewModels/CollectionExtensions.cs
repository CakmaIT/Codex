using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ozge.App.Presentation.ViewModels;

internal static class CollectionExtensions
{
    public static void ReplaceWith<T>(this ObservableCollection<T> source, IEnumerable<T> items)
    {
        source.Clear();
        foreach (var item in items)
        {
            source.Add(item);
        }
    }
}
