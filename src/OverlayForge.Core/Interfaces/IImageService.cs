namespace OverlayForge.Core.Interfaces;

/// <summary>
/// Service for loading images from disk.
/// </summary>
public interface IImageService
{
    /// <summary>Validates that a file is a supported image format.</summary>
    bool IsValidImageFile(string path);

    /// <summary>Gets supported file extensions.</summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>Gets the image dimensions without fully loading it.</summary>
    Task<(int Width, int Height)> GetImageDimensionsAsync(string path);
}
