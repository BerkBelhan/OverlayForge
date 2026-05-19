using System.Windows;
using OverlayForge.UI.ViewModels;
using OverlayForge.Win32.Helpers;

namespace OverlayForge.UI.Views;

/// <summary>
/// Settings window code-behind.
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel settingsViewModel)
    {
        DataContext = settingsViewModel;
        InitializeComponent();
        Loaded += (_, _) => WindowStyleHelper.SetDarkMode(this, true);
    }
}
