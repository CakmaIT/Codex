using System.IO;

namespace Ozge.App.Services;

public sealed class AppPaths(string root)
{
    public string Root { get; } = root;
    public string Database => Path.Combine(Root, "ozge2.db");
    public string Logs => Path.Combine(Root, "Logs");
    public string Backups => Path.Combine(Root, "Backups");
    public string Snapshots => Path.Combine(Root, "Snapshots");
    public string TessData => Path.Combine(Root, "tessdata");
}
