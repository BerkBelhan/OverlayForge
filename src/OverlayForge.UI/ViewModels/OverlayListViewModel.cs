using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;

namespace OverlayForge.UI.ViewModels;

/// <summary>
/// ViewModel for the overlay list panel.
/// </summary>
public sealed partial class OverlayListViewModel : ObservableObject
{
    private readonly IOverlayManager _overlayManager;
    private readonly IImageService _imageService;
    private readonly ILogger<OverlayListViewModel> _logger;

    public ObservableCollection<OverlayItemViewModel> Overlays { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedOverlay))]
    private OverlayItemViewModel? _selectedOverlay;

    public bool HasSelectedOverlay => SelectedOverlay is not null;

    public OverlayListViewModel(
        IOverlayManager overlayManager,
        IImageService imageService,
        ILogger<OverlayListViewModel> logger)
    {
        _overlayManager = overlayManager;
        _imageService = imageService;
        _logger = logger;

        _overlayManager.OverlayChanged += OnOverlayChanged;
    }

    private void OnOverlayChanged(object? sender, OverlayChangedEventArgs e)
    {
        // Ensure we update on the UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            switch (e.ChangeType)
            {
                case OverlayChangeType.Added:
                    Overlays.Add(new OverlayItemViewModel(e.Overlay, _overlayManager));
                    break;

                case OverlayChangeType.Removed:
                    var toRemove = Overlays.FirstOrDefault(o => o.Id == e.Overlay.Id);
                    if (toRemove is not null) Overlays.Remove(toRemove);
                    break;

                case OverlayChangeType.Updated:
                    var toUpdate = Overlays.FirstOrDefault(o => o.Id == e.Overlay.Id);
                    toUpdate?.SyncFromModel(e.Overlay);
                    break;
            }
        });
    }

    [RelayCommand]
    private void AddEmptyOverlay()
    {
        _overlayManager.CreateOverlay();
        _logger.LogInformation("Added empty overlay via UI.");
    }

    [RelayCommand]
    private void AddOverlayFromFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select an image for overlay",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.webp;*.bmp)|*.png;*.jpg;*.jpeg;*.webp;*.bmp|All files (*.*)|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            if (!_imageService.IsValidImageFile(dialog.FileName))
            {
                System.Windows.MessageBox.Show(
                    "The selected file is not a supported image format.",
                    "Invalid File",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            _overlayManager.CreateOverlay(dialog.FileName);
            _logger.LogInformation("Added overlay from file: {Path}", dialog.FileName);
        }
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        if (SelectedOverlay is null) return;
        _overlayManager.RemoveOverlay(SelectedOverlay.Id);
    }

    [RelayCommand]
    private void HideAll() => _overlayManager.HideAll();

    [RelayCommand]
    private void ShowAll() => _overlayManager.ShowAll();
}
