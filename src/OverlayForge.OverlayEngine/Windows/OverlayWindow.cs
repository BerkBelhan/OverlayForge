using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using OverlayForge.Core.Models;
using OverlayForge.Win32.Helpers;
using OverlayForge.Win32.Interop;

namespace OverlayForge.OverlayEngine.Windows;

/// <summary>
/// A borderless, always-on-top WPF window used as an image overlay.
/// Supports transparency, click-through, drag, resize, and lock modes.
/// </summary>
public sealed class OverlayWindow : Window
{
    private readonly ILogger<OverlayWindow> _logger;
    private OverlayModel _model;

    private bool _isDragging;
    private Point _dragOffset;
    private bool _isResizing;
    private Point _resizeStart;
    private Size _resizeStartSize;

    private readonly Image _image;
    private readonly Border _border;
    private readonly Grid _rootGrid;

    public event EventHandler<OverlayModel>? ModelUpdated;
    public event EventHandler? CloseRequested;

    public OverlayModel Model => _model;

    public OverlayWindow(OverlayModel model, ILogger<OverlayWindow> logger)
    {
        _model = model;
        _logger = logger;

        // Configure window appearance
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        Topmost = true;
        ResizeMode = ResizeMode.NoResize; // We handle resize manually
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        // Set initial position and size from model
        Left = model.X;
        Top = model.Y;
        Width = model.Width;
        Height = model.Height;

        // Build visual tree
        _image = new Image
        {
            Stretch = Stretch.Fill,
            StretchDirection = StretchDirection.Both,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        UpdateImageTransforms();

        _border = new Border
        {
            Background = Brushes.Transparent,
            Child = _image
        };

        // Resize grip overlay (bottom-right corner)
        var resizeGrip = new System.Windows.Shapes.Rectangle
        {
            Width = 16,
            Height = 16,
            Fill = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Cursor = Cursors.SizeNWSE,
            ToolTip = "Drag to resize"
        };
        resizeGrip.MouseLeftButtonDown += ResizeGrip_MouseLeftButtonDown;

        _rootGrid = new Grid();
        _rootGrid.Children.Add(_border);
        _rootGrid.Children.Add(resizeGrip);

        Content = _rootGrid;

        // Wire up drag events on the image itself
        _image.MouseLeftButtonDown += Image_MouseLeftButtonDown;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowStyleHelper.MakeOverlayWindow(this);
        ApplyModel();
    }

    /// <summary>
    /// Loads an image from the model's ImagePath.
    /// </summary>
    public void LoadImage()
    {
        if (string.IsNullOrEmpty(_model.ImagePath) || !File.Exists(_model.ImagePath))
        {
            _image.Source = null;
            return;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(_model.ImagePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze(); // Freeze for cross-thread use and performance
            _image.Source = bitmap;

            // Auto-size if model dimensions are default
            if (_model.Width <= 10 || _model.Height <= 10)
            {
                _model.Width = bitmap.PixelWidth;
                _model.Height = bitmap.PixelHeight;
                Width = _model.Width;
                Height = _model.Height;
            }

            _logger.LogDebug("Loaded image {Path} ({W}x{H})", _model.ImagePath, bitmap.PixelWidth, bitmap.PixelHeight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load image {Path}", _model.ImagePath);
        }
    }

    /// <summary>
    /// Applies current model state to the window.
    /// </summary>
    public void ApplyModel()
    {
        if (!IsLoaded) return;

        Left = _model.X;
        Top = _model.Y;
        Width = _model.Width;
        Height = _model.Height;

        Visibility = _model.IsVisible ? Visibility.Visible : Visibility.Collapsed;

        WindowStyleHelper.SetWindowOpacity(this, _model.Opacity);
        WindowStyleHelper.SetClickThrough(this, _model.IsClickThrough);

        UpdateImageTransforms();
        LoadImage();
    }

    /// <summary>
    /// Updates the model and re-applies to UI.
    /// </summary>
    public void UpdateModel(OverlayModel model)
    {
        _model = model;
        ApplyModel();
    }

    private void UpdateImageTransforms()
    {
        var group = new TransformGroup();

        if (_model.FlipHorizontal || _model.FlipVertical)
        {
            group.Children.Add(new ScaleTransform(
                _model.FlipHorizontal ? -1 : 1,
                _model.FlipVertical ? -1 : 1));
        }

        if (_model.Rotation != 0)
            group.Children.Add(new RotateTransform(_model.Rotation));

        _image.RenderTransform = group.Children.Count > 0 ? group : Transform.Identity;
    }

    // ─── Dragging ─────────────────────────────────────────────────────────────

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_model.IsLocked || _model.IsClickThrough) return;

        _isDragging = true;
        _dragOffset = e.GetPosition(this);
        _image.CaptureMouse();
        _image.MouseMove += Image_MouseMove;
        _image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var screenPos = PointToScreen(e.GetPosition(this));
        Left = screenPos.X - _dragOffset.X;
        Top = screenPos.Y - _dragOffset.Y;
    }

    private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        _image.ReleaseMouseCapture();
        _image.MouseMove -= Image_MouseMove;
        _image.MouseLeftButtonUp -= Image_MouseLeftButtonUp;

        // Persist new position to model
        _model.X = Left;
        _model.Y = Top;
        _model.ModifiedAt = DateTime.UtcNow;
        ModelUpdated?.Invoke(this, _model);
    }

    // ─── Resizing ─────────────────────────────────────────────────────────────

    private void ResizeGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_model.IsLocked) return;
        _isResizing = true;
        _resizeStart = PointToScreen(e.GetPosition(this));
        _resizeStartSize = new Size(Width, Height);

        var grip = (UIElement)sender;
        grip.CaptureMouse();
        grip.MouseMove += ResizeGrip_MouseMove;
        grip.MouseLeftButtonUp += ResizeGrip_MouseLeftButtonUp;
        e.Handled = true;
    }

    private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isResizing) return;
        var current = PointToScreen(e.GetPosition(this));
        var delta = current - _resizeStart;

        double newWidth = Math.Max(50, _resizeStartSize.Width + delta.X);
        double newHeight;

        if (_model.MaintainAspectRatio && _resizeStartSize.Width > 0)
        {
            newHeight = newWidth / _resizeStartSize.Width * _resizeStartSize.Height;
        }
        else
        {
            newHeight = Math.Max(50, _resizeStartSize.Height + delta.Y);
        }

        Width = newWidth;
        Height = newHeight;
    }

    private void ResizeGrip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isResizing) return;
        _isResizing = false;
        var grip = (UIElement)sender;
        grip.ReleaseMouseCapture();
        grip.MouseMove -= ResizeGrip_MouseMove;
        grip.MouseLeftButtonUp -= ResizeGrip_MouseLeftButtonUp;

        _model.Width = Width;
        _model.Height = Height;
        _model.ModifiedAt = DateTime.UtcNow;
        ModelUpdated?.Invoke(this, _model);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}


