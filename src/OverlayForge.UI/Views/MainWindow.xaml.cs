using System.Windows;
using System.Windows.Forms;
using OverlayForge.UI.ViewModels;
using OverlayForge.Win32.Helpers;
using System.Drawing;

namespace OverlayForge.UI.Views;

/// <summary>
/// Main application window code-behind.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private NotifyIcon? _trayIcon;

    public MainWindow(MainViewModel viewModel, SettingsViewModel settingsViewModel)
    {
        _viewModel = viewModel;
        _settingsViewModel = settingsViewModel;
        DataContext = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowStyleHelper.SetDarkMode(this, true);
        SetupTrayIcon();
        await _viewModel.InitializeAsync();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Ask whether to close or minimize to tray
        var result = System.Windows.MessageBox.Show(
            "Minimize OverlayForge to tray instead of closing?\n\nAll overlays will remain visible.",
            "OverlayForge",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel)
        {
            e.Cancel = true;
        }
        else if (result == MessageBoxResult.Yes)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            _trayIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Text = "OverlayForge",
            Icon = SystemIcons.Application,
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show OverlayForge", null, (_, _) => ShowMainWindow());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, (_, _) =>
        {
            _trayIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        });

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_settingsViewModel)
        {
            Owner = this
        };
        settingsWindow.ShowDialog();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
