using Serilog;
using Serilog.Events;

namespace OverlayForge.Infrastructure.Logging;

/// <summary>
/// Configures Serilog for OverlayForge.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Creates and configures a Serilog logger.
    /// </summary>
    public static ILogger CreateLogger(string logLevel = "Information", string? logDirectory = null)
    {
        logDirectory ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OverlayForge",
            "logs");

        Directory.CreateDirectory(logDirectory);

        var level = ParseLogLevel(logLevel);

        return new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "OverlayForge")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logDirectory, "overlayforge-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static LogEventLevel ParseLogLevel(string level) => level.ToLowerInvariant() switch
    {
        "verbose" => LogEventLevel.Verbose,
        "debug" => LogEventLevel.Debug,
        "warning" => LogEventLevel.Warning,
        "error" => LogEventLevel.Error,
        "fatal" => LogEventLevel.Fatal,
        _ => LogEventLevel.Information
    };
}
