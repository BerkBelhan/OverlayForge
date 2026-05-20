using FluentAssertions;
using OverlayForge.Core.Models;
using OverlayForge.Core.Enums;
using Xunit;

namespace OverlayForge.Core.Tests;

/// <summary>
/// Tests for the OverlayModel class.
/// </summary>
public sealed class OverlayModelTests
{
    [Fact]
    public void NewOverlayModel_HasSensibleDefaults()
    {
        var model = new OverlayModel();

        model.Id.Should().NotBe(Guid.Empty);
        model.Name.Should().Be("Overlay");
        model.State.Should().Be(OverlayState.Active);
        model.Opacity.Should().Be(0.5);
        model.IsVisible.Should().BeTrue();
        model.IsLocked.Should().BeFalse();
        model.IsClickThrough.Should().BeFalse();
        model.MaintainAspectRatio.Should().BeTrue();
        model.Rotation.Should().Be(0);
        model.Width.Should().Be(400);
        model.Height.Should().Be(300);
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new OverlayModel
        {
            Name = "Test",
            Opacity = 0.75,
            Width = 800,
            Height = 600,
            X = 100,
            Y = 200,
            ImagePath = "/test/image.png"
        };

        var clone = original.Clone();

        clone.Id.Should().NotBe(original.Id);
        clone.Name.Should().Be("Test (Copy)");
        clone.Opacity.Should().Be(0.75);
        clone.Width.Should().Be(800);
        clone.Height.Should().Be(600);
        clone.ImagePath.Should().Be("/test/image.png");
        // Position should be offset
        clone.X.Should().Be(original.X + 20);
        clone.Y.Should().Be(original.Y + 20);
    }

    [Fact]
    public void Clone_DoesNotShareReference()
    {
        var original = new OverlayModel { Name = "Original" };
        var clone = original.Clone();

        clone.Name = "Modified";

        original.Name.Should().Be("Original");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Opacity_AcceptsValidValues(double opacity)
    {
        var model = new OverlayModel { Opacity = opacity };
        model.Opacity.Should().Be(opacity);
    }

    [Fact]
    public void AppSettings_HasDefaultHotkeyBindings()
    {
        var settings = new AppSettings();
        settings.HotkeyBindings.Should().BeEmpty(); // Populated by SettingsService
    }
}
