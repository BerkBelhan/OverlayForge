namespace OverlayForge.Core.Interfaces;

/// <summary>
/// Abstraction for file system operations (testable).
/// </summary>
public interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
    Task<byte[]> ReadAllBytesAsync(string path);
    void DeleteFile(string path);
    IEnumerable<string> GetFiles(string directory, string searchPattern);
    string GetFileName(string path);
    string GetDirectoryName(string path);
    string Combine(params string[] paths);
    string GetExtension(string path);
}
