using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OverlayForge.Core.Interfaces;
using OverlayForge.Infrastructure.Services;
using Xunit;

namespace OverlayForge.Infrastructure.Tests;

/// <summary>
/// Tests for ImageService.
/// </summary>
public sealed class ImageServiceTests
{
    private readonly Mock<IFileSystem> _fileSystemMock = new();
    private readonly NullLogger<ImageService> _logger = new();

    [Theory]
    [InlineData(".png", true)]
    [InlineData(".jpg", true)]
    [InlineData(".jpeg", true)]
    [InlineData(".webp", true)]
    [InlineData(".bmp", true)]
    [InlineData(".txt", false)]
    [InlineData(".exe", false)]
    [InlineData(".gif", false)]
    public void IsValidImageFile_ReturnsCorrectResult(string extension, bool expected)
    {
        var path = $"/test/image{extension}";
        _fileSystemMock.Setup(fs => fs.FileExists(path)).Returns(true);
        _fileSystemMock.Setup(fs => fs.GetExtension(path)).Returns(extension);

        var service = new ImageService(_fileSystemMock.Object, _logger);
        var result = service.IsValidImageFile(path);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidImageFile_ReturnsFalse_WhenFileDoesNotExist()
    {
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        var service = new ImageService(_fileSystemMock.Object, _logger);
        var result = service.IsValidImageFile("/nonexistent.png");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_ReturnsFalse_ForNullOrEmpty()
    {
        var service = new ImageService(_fileSystemMock.Object, _logger);

        service.IsValidImageFile(null!).Should().BeFalse();
        service.IsValidImageFile("").Should().BeFalse();
        service.IsValidImageFile("   ").Should().BeFalse();
    }

    [Fact]
    public void SupportedExtensions_ContainsExpectedFormats()
    {
        var service = new ImageService(_fileSystemMock.Object, _logger);

        service.SupportedExtensions.Should().Contain(".png");
        service.SupportedExtensions.Should().Contain(".jpg");
        service.SupportedExtensions.Should().Contain(".jpeg");
        service.SupportedExtensions.Should().Contain(".bmp");
        service.SupportedExtensions.Should().Contain(".webp");
    }

    [Fact]
    public async Task GetImageDimensionsAsync_ValidPng_ReturnsDimensions()
    {
        // Minimal PNG: 8 bytes signature + IHDR with width=100, height=50
        var pngBytes = CreateMinimalPngHeader(100, 50);

        _fileSystemMock.Setup(fs => fs.ReadAllBytesAsync("/test/image.png"))
            .ReturnsAsync(pngBytes);
        _fileSystemMock.Setup(fs => fs.GetExtension("/test/image.png"))
            .Returns(".png");

        var service = new ImageService(_fileSystemMock.Object, _logger);
        var (width, height) = await service.GetImageDimensionsAsync("/test/image.png");

        width.Should().Be(100);
        height.Should().Be(50);
    }

    [Fact]
    public async Task GetImageDimensionsAsync_ReturnsZero_OnException()
    {
        _fileSystemMock.Setup(fs => fs.ReadAllBytesAsync(It.IsAny<string>()))
            .ThrowsAsync(new IOException("File not found"));
        _fileSystemMock.Setup(fs => fs.GetExtension(It.IsAny<string>())).Returns(".png");

        var service = new ImageService(_fileSystemMock.Object, _logger);
        var (width, height) = await service.GetImageDimensionsAsync("/missing.png");

        width.Should().Be(0);
        height.Should().Be(0);
    }

    [Fact]
    public async Task GetImageDimensionsAsync_ValidBmp_ReturnsDimensions()
    {
        var bmpBytes = CreateMinimalBmpHeader(200, 150);
        _fileSystemMock.Setup(fs => fs.ReadAllBytesAsync("/test/image.bmp"))
            .ReturnsAsync(bmpBytes);
        _fileSystemMock.Setup(fs => fs.GetExtension("/test/image.bmp"))
            .Returns(".bmp");

        var service = new ImageService(_fileSystemMock.Object, _logger);
        var (width, height) = await service.GetImageDimensionsAsync("/test/image.bmp");

        width.Should().Be(200);
        height.Should().Be(150);
    }

    private static byte[] CreateMinimalPngHeader(int width, int height)
    {
        var bytes = new byte[30];
        // PNG signature
        bytes[0] = 0x89; bytes[1] = 0x50; bytes[2] = 0x4E; bytes[3] = 0x47;
        bytes[4] = 0x0D; bytes[5] = 0x0A; bytes[6] = 0x1A; bytes[7] = 0x0A;
        // IHDR chunk (12 bytes preamble + data)
        // Width at byte 16-19 (big endian)
        bytes[16] = (byte)(width >> 24);
        bytes[17] = (byte)(width >> 16);
        bytes[18] = (byte)(width >> 8);
        bytes[19] = (byte)width;
        // Height at byte 20-23 (big endian)
        bytes[20] = (byte)(height >> 24);
        bytes[21] = (byte)(height >> 16);
        bytes[22] = (byte)(height >> 8);
        bytes[23] = (byte)height;
        return bytes;
    }

    private static byte[] CreateMinimalBmpHeader(int width, int height)
    {
        var bytes = new byte[30];
        // BM signature
        bytes[0] = 0x42; bytes[1] = 0x4D;
        // Width at offset 18-21 (little endian)
        bytes[18] = (byte)width;
        bytes[19] = (byte)(width >> 8);
        bytes[20] = (byte)(width >> 16);
        bytes[21] = (byte)(width >> 24);
        // Height at offset 22-25 (little endian)
        bytes[22] = (byte)height;
        bytes[23] = (byte)(height >> 8);
        bytes[24] = (byte)(height >> 16);
        bytes[25] = (byte)(height >> 24);
        return bytes;
    }
}
