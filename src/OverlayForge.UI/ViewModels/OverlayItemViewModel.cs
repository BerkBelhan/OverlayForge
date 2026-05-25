using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;

namespace OverlayForge.UI.ViewModels;

/// <summary>
/// ViewModel for overlay list item display and actions.
/// </summary>
public sealed partial class OverlayItemViewModel : ObservableObject
{
    public OverlayModel Model { get; }
    private readonly IOverlayManager _overlayManager;

    [ObservableProperty] private string _name;
    [ObservableProperty] private double _opacity;
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private bool _isLocked;
    [ObservableProperty] private bool _isClickThrough;
    [ObservableProperty] private string? _imagePath;
    [ObservableProperty] private bool _isSelected;

    public Guid Id { get; }

    public OverlayItemViewModel(OverlayModel model, IOverlayManager overlayManager)
    {
        _overlayManager = overlayManager;
        Model = model;
        Id = model.Id;
        _name = model.Name;
        _opacity = model.Opacity;
        _isVisible = model.IsVisible;
        _isLocked = model.IsLocked;
        _isClickThrough = model.IsClickThrough;
        _imagePath = model.ImagePath;
    }

    partial void OnOpacityChanged(double value)
    {
        _overlayManager.SetOpacity(Id, value);
    }

    [RelayCommand]
    private void ToggleVisibility() => _overlayManager.ToggleVisibility(Id);

    [RelayCommand]
    private void ToggleLock() => _overlayManager.ToggleLock(Id);

    [RelayCommand]
    private void ToggleClickThrough() => _overlayManager.ToggleClickThrough(Id);

    [RelayCommand]
    private void Remove() => _overlayManager.RemoveOverlay(Id);

    [RelayCommand]
    private void Duplicate() => _overlayManager.DuplicateOverlay(Id);

    [RelayCommand]
    private void BringToFront() => _overlayManager.BringToFront(Id);

    [RelayCommand]
    private void SendToBack() => _overlayManager.SendToBack(Id);

    /// <summary>Updates this viewmodel from the backing model.</summary>
    public void SyncFromModel(OverlayModel model)
    {
        Name = model.Name;
        Opacity = model.Opacity;
        IsVisible = model.IsVisible;
        IsLocked = model.IsLocked;
        IsClickThrough = model.IsClickThrough;
        ImagePath = model.ImagePath;
    }
}
