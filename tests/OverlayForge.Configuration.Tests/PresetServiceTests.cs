using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OverlayForge.Configuration.Services;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;
using Xunit;

namespace OverlayForge.Configuration.Tests;

/// <summary>
/// Tests for PresetService.
/// </summary>
public sealed class PresetServiceTests
{
    private readonly Mock<IFileSystem> _fileSystemMock = new();
    private readonly NullLogger<PresetService> _logger = new();
    private const string PresetsDir = "/fake/presets";

    [Fact]
    public async Task LoadPresetsAsync_EmptyDirectory_LoadsNoPresets()
    {
        _fileSystemMock.Setup(fs => fs.DirectoryExists(PresetsDir)).Returns(true);
        _fileSystemMock.Setup(fs => fs.GetFiles(PresetsDir, "*.json"))
            .Returns(Enumerable.Empty<string>());

        var service = new PresetService(_fileSystemMock.Object, _logger, PresetsDir);
        await service.LoadPresetsAsync();

        service.Presets.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadPresetsAsync_WithValidFile_LoadsPreset()
    {
        var preset = new OverlayPreset { Name = "Test Preset", Overlays = new() };
        var json = System.Text.Json.JsonSerializer.Serialize(preset,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            });

        _fileSystemMock.Setup(fs => fs.DirectoryExists(PresetsDir)).Returns(true);
        _fileSystemMock.Setup(fs => fs.GetFiles(PresetsDir, "*.json"))
            .Returns(new[] { $"{PresetsDir}/{preset.Id}.json" });
        _fileSystemMock.Setup(fs => fs.ReadAllTextAsync(It.IsAny<string>()))
            .ReturnsAsync(json);

        var service = new PresetService(_fileSystemMock.Object, _logger, PresetsDir);
        await service.LoadPresetsAsync();

        service.Presets.Should().HaveCount(1);
        service.Presets[0].Name.Should().Be("Test Preset");
    }

    [Fact]
    public async Task SavePresetAsync_PersistsToFileSystem()
    {
        string? writtenJson = null;
        _fileSystemMock.Setup(fs => fs.DirectoryExists(PresetsDir)).Returns(true);
        _fileSystemMock.Setup(fs => fs.Combine(It.IsAny<string[]>()))
            .Returns<string[]>(parts => string.Join("/", parts));
        _fileSystemMock.Setup(fs => fs.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((_, json) => writtenJson = json)
            .Returns(Task.CompletedTask);

        var service = new PresetService(_fileSystemMock.Object, _logger, PresetsDir);
        var overlays = new List<OverlayModel>
        {
            new() { Name = "Overlay 1" }
        };

        var preset = await service.SavePresetAsync("My Preset", overlays, "Test description");

        preset.Name.Should().Be("My Preset");
        preset.Description.Should().Be("Test description");
        preset.Overlays.Should().HaveCount(1);
        writtenJson.Should().Contain("My Preset");
        service.Presets.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeletePresetAsync_RemovesFromCollection()
    {
        _fileSystemMock.Setup(fs => fs.DirectoryExists(PresetsDir)).Returns(true);
        _fileSystemMock.Setup(fs => fs.GetFiles(PresetsDir, "*.json"))
            .Returns(Enumerable.Empty<string>());
        _fileSystemMock.Setup(fs => fs.Combine(It.IsAny<string[]>()))
            .Returns<string[]>(parts => string.Join("/", parts));
        _fileSystemMock.Setup(fs => fs.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        _fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>()));

        var service = new PresetService(_fileSystemMock.Object, _logger, PresetsDir);
        var preset = await service.SavePresetAsync("Delete Me", Enumerable.Empty<OverlayModel>());

        var result = await service.DeletePresetAsync(preset.Id);

        result.Should().BeTrue();
        service.Presets.Should().BeEmpty();
    }

    [Fact]
    public void FindAutoLoadPreset_ReturnsMatchingPreset()
    {
        _fileSystemMock.Setup(fs => fs.DirectoryExists(PresetsDir)).Returns(true);
        _fileSystemMock.Setup(fs => fs.GetFiles(PresetsDir, "*.json"))
            .Returns(Enumerable.Empty<string>());
        _fileSystemMock.Setup(fs => fs.Combine(It.IsAny<string[]>()))
            .Returns<string[]>(parts => string.Join("/", parts));
        _fileSystemMock.Setup(fs => fs.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new PresetService(_fileSystemMock.Object, _logger, PresetsDir);

        // Manually add preset with auto-load executable
        var preset = new OverlayPreset
        {
            Name = "Game Preset",
            AutoLoadExecutable = "game.exe"
        };

        // Access internal list via reflection for testing
        var presetsField = typeof(PresetService).GetField("_presets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ((List<OverlayPreset>)presetsField!.GetValue(service)!).Add(preset);

        var found = service.FindAutoLoadPreset("game.exe");
        found.Should().NotBeNull();
        found!.Name.Should().Be("Game Preset");
    }

    [Fact]
    public void FindAutoLoadPreset_CaseInsensitive()
    {
        _fileSystemMock.Setup(fs => fs.DirectoryExists(PresetsDir)).Returns(true);

        var service = new PresetService(_fileSystemMock.Object, _logger, PresetsDir);

        var preset = new OverlayPreset
        {
            AutoLoadExecutable = "GAME.EXE"
        };

        var presetsField = typeof(PresetService).GetField("_presets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ((List<OverlayPreset>)presetsField!.GetValue(service)!).Add(preset);

        var found = service.FindAutoLoadPreset("game.exe");
        found.Should().NotBeNull();
    }
}
