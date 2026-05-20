using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OverlayForge.Configuration.Services;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;
using Xunit;

namespace OverlayForge.Configuration.Tests;

/// <summary>
/// Tests for SettingsService.
/// </summary>
public sealed class SettingsServiceTests
{
    private readonly Mock<IFileSystem> _fileSystemMock = new();
    private readonly NullLogger<SettingsService> _logger = new();

    [Fact]
    public async Task LoadAsync_WhenNoFile_CreatesDefaultSettings()
    {
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);
        _fileSystemMock.Setup(fs => fs.GetDirectoryName(It.IsAny<string>())).Returns("/fake/dir");
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        _fileSystemMock.Setup(fs => fs.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        await service.LoadAsync();

        service.Settings.Should().NotBeNull();
        service.Settings.SchemaVersion.Should().Be("1.0");
        service.Settings.OpacityStep.Should().Be(0.05);
        service.Settings.HotkeyBindings.Should().HaveCount(5);
    }

    [Fact]
    public async Task SaveAsync_WritesJsonToFile()
    {
        string? writtenJson = null;
        _fileSystemMock.Setup(fs => fs.GetDirectoryName("/fake/settings.json")).Returns("/fake");
        _fileSystemMock.Setup(fs => fs.DirectoryExists("/fake")).Returns(true);
        _fileSystemMock.Setup(fs => fs.WriteAllTextAsync("/fake/settings.json", It.IsAny<string>()))
            .Callback<string, string>((_, json) => writtenJson = json)
            .Returns(Task.CompletedTask);

        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        service.Settings.OpacityStep = 0.1;
        await service.SaveAsync();

        writtenJson.Should().NotBeNull();
        writtenJson.Should().Contain("0.1");
    }

    [Fact]
    public async Task LoadAsync_WithValidJson_RestoresSettings()
    {
        var json = """
            {
              "SchemaVersion": "1.0",
              "OpacityStep": 0.08,
              "SnapToEdges": false,
              "HotkeyBindings": []
            }
            """;

        _fileSystemMock.Setup(fs => fs.FileExists("/fake/settings.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllTextAsync("/fake/settings.json"))
            .ReturnsAsync(json);

        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        await service.LoadAsync();

        service.Settings.OpacityStep.Should().Be(0.08);
        service.Settings.SnapToEdges.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithInvalidJson_FallsBackToDefaults()
    {
        _fileSystemMock.Setup(fs => fs.FileExists("/fake/settings.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllTextAsync("/fake/settings.json"))
            .ReturnsAsync("{ invalid json !!!");

        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        await service.LoadAsync();

        service.Settings.Should().NotBeNull();
        service.Settings.OpacityStep.Should().Be(0.05); // default
    }

    [Fact]
    public void AddRecentImage_AddsToFrontOfList()
    {
        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        service.Settings.RecentImages.Add("/old/image.png");

        service.AddRecentImage("/new/image.png");

        service.Settings.RecentImages.First().Should().Be("/new/image.png");
    }

    [Fact]
    public void AddRecentImage_RemovesDuplicates()
    {
        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        service.Settings.RecentImages.Add("/test/image.png");
        service.Settings.RecentImages.Add("/other/image.png");

        service.AddRecentImage("/test/image.png");

        service.Settings.RecentImages.Should().HaveCount(2);
        service.Settings.RecentImages.First().Should().Be("/test/image.png");
    }

    [Fact]
    public void AddRecentImage_LimitsListTo20()
    {
        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        for (int i = 0; i < 25; i++)
            service.Settings.RecentImages.Add($"/image{i}.png");

        service.AddRecentImage("/new.png");

        service.Settings.RecentImages.Should().HaveCount(20);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_RestoresDefaults()
    {
        _fileSystemMock.Setup(fs => fs.GetDirectoryName(It.IsAny<string>())).Returns("/fake");
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        _fileSystemMock.Setup(fs => fs.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new SettingsService(_fileSystemMock.Object, _logger, "/fake/settings.json");
        service.Settings.OpacityStep = 0.99;

        await service.ResetToDefaultsAsync();

        service.Settings.OpacityStep.Should().Be(0.05);
    }
}
