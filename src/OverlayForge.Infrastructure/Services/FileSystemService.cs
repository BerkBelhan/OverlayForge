using OverlayForge.Core.Interfaces;

namespace OverlayForge.Infrastructure.Services;

/// <summary>
/// Concrete file system implementation using System.IO.
/// </summary>
public sealed class FileSystemService : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);

    public Task WriteAllTextAsync(string path, string content) => File.WriteAllTextAsync(path, content);

    public Task<byte[]> ReadAllBytesAsync(string path) => File.ReadAllBytesAsync(path);

    public void DeleteFile(string path) => File.Delete(path);

    public IEnumerable<string> GetFiles(string directory, string searchPattern) =>
        Directory.GetFiles(directory, searchPattern);

    public string GetFileName(string path) => Path.GetFileName(path);

    public string GetDirectoryName(string path) => Path.GetDirectoryName(path) ?? string.Empty;

    public string Combine(params string[] paths) => Path.Combine(paths);

    public string GetExtension(string path) => Path.GetExtension(path);
}
