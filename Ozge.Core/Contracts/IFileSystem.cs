namespace Ozge.Core.Contracts;

public interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    Stream OpenWrite(string path);
    Stream OpenRead(string path);
    void CopyFile(string source, string destination, bool overwrite);
    Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
}
