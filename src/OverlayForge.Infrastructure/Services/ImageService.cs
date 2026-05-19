using Microsoft.Extensions.Logging;
using OverlayForge.Core.Interfaces;

namespace OverlayForge.Infrastructure.Services;

/// <summary>
/// Service for validating and querying image files.
/// Cross-platform compatible implementation using file header inspection.
/// </summary>
public sealed class ImageService : IImageService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ImageService> _logger;

    private static readonly string[] SupportedExtensionsArray =
        { ".png", ".jpg", ".jpeg", ".webp", ".bmp" };

    public IReadOnlyList<string> SupportedExtensions => SupportedExtensionsArray;

    public ImageService(IFileSystem fileSystem, ILogger<ImageService> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public bool IsValidImageFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !_fileSystem.FileExists(path))
            return false;

        var ext = _fileSystem.GetExtension(path).ToLowerInvariant();
        return SupportedExtensionsArray.Contains(ext);
    }

    public async Task<(int Width, int Height)> GetImageDimensionsAsync(string path)
    {
        try
        {
            var bytes = await _fileSystem.ReadAllBytesAsync(path);
            var ext = _fileSystem.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".png" => ReadPngDimensions(bytes),
                ".jpg" or ".jpeg" => ReadJpegDimensions(bytes),
                ".bmp" => ReadBmpDimensions(bytes),
                _ => (0, 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read dimensions for {Path}", path);
            return (0, 0);
        }
    }

    private static (int Width, int Height) ReadPngDimensions(byte[] bytes)
    {
        // PNG: signature 8 bytes, then IHDR chunk - width at offset 16, height at 20
        if (bytes.Length < 24) return (0, 0);
        // Verify PNG signature
        if (bytes[0] != 0x89 || bytes[1] != 0x50) return (0, 0);

        int width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
        int height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
        return (width, height);
    }

    private static (int Width, int Height) ReadJpegDimensions(byte[] bytes)
    {
        // Parse JPEG SOF markers
        int i = 2;
        while (i < bytes.Length - 8)
        {
            if (bytes[i] != 0xFF) break;
            byte marker = bytes[i + 1];

            // SOF markers: 0xC0-0xC3, 0xC5-0xC7, 0xC9-0xCB, 0xCD-0xCF
            if (marker is >= 0xC0 and <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC)
            {
                int height = (bytes[i + 5] << 8) | bytes[i + 6];
                int width = (bytes[i + 7] << 8) | bytes[i + 8];
                return (width, height);
            }

            int length = (bytes[i + 2] << 8) | bytes[i + 3];
            i += 2 + length;
        }
        return (0, 0);
    }

    private static (int Width, int Height) ReadBmpDimensions(byte[] bytes)
    {
        if (bytes.Length < 26) return (0, 0);
        // BMP: 'BM' signature, width at offset 18, height at 22 (little-endian)
        if (bytes[0] != 0x42 || bytes[1] != 0x4D) return (0, 0);
        int width = bytes[18] | (bytes[19] << 8) | (bytes[20] << 16) | (bytes[21] << 24);
        int height = bytes[22] | (bytes[23] << 8) | (bytes[24] << 16) | (bytes[25] << 24);
        return (width, Math.Abs(height));
    }
}
