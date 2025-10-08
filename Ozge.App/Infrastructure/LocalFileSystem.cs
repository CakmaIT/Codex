using System.IO;
using Ozge.Core.Contracts;

namespace Ozge.App.Infrastructure;

public sealed class LocalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public Stream OpenWrite(string path) => File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public void CopyFile(string source, string destination, bool overwrite) => File.Copy(source, destination, overwrite);

    public Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken) =>
        File.WriteAllBytesAsync(path, data.ToArray(), cancellationToken);
}
