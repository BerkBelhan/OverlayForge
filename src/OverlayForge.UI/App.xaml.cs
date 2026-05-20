using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OverlayForge.Configuration.Services;
using OverlayForge.Core.Interfaces;
using OverlayForge.Hotkeys.Services;
using OverlayForge.Infrastructure.Logging;
using OverlayForge.Infrastructure.Services;
using OverlayForge.OverlayEngine.Services;
using OverlayForge.UI.ViewModels;
using OverlayForge.UI.Views;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace OverlayForge.UI;

/// <summary>
/// Application entry point with dependency injection setup.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog first (before DI)
        var serilogLogger = LoggingConfiguration.CreateLogger();
        Log.Logger = serilogLogger;

        try
        {
            _serviceProvider = ConfigureServices(serilogLogger);

            // Load settings
            var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            await settingsService.LoadAsync();

            // Register hotkeys
            var hotkeyService = _serviceProvider.GetRequiredService<IHotkeyService>();
            hotkeyService.RegisterHotkeys(settingsService.Settings.HotkeyBindings);

            // Show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Log.Information("OverlayForge started successfully.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed.");
            MessageBox.Show(
                $"OverlayForge failed to start:\n\n{ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Cleanup
        if (_serviceProvider is not null)
        {
            try
            {
                var hotkeyService = _serviceProvider.GetRequiredService<IHotkeyService>();
                hotkeyService.UnregisterAll();
            }
            catch { /* ignore cleanup errors */ }

            _serviceProvider.Dispose();
        }

        Log.Information("OverlayForge shutting down.");
        Log.CloseAndFlush();

        base.OnExit(e);
    }

    private static ServiceProvider ConfigureServices(Serilog.ILogger serilogLogger)
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(serilogLogger, dispose: false);
        });

        // Core infrastructure
        services.AddSingleton<IFileSystem, FileSystemService>();
        services.AddSingleton<IImageService, ImageService>();

        // Configuration services
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPresetService, PresetService>();

        // Engine services
        services.AddSingleton<IOverlayManager, OverlayManager>();
        services.AddSingleton<IHotkeyService, HotkeyService>();

        // View models
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<OverlayListViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddTransient<SettingsWindow>();

        return services.BuildServiceProvider();
    }
}
