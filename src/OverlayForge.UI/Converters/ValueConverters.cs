using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OverlayForge.UI.Converters;

/// <summary>Converts bool to Visibility (True = Visible, False = Collapsed).</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter is string s && s == "invert";
        bool boolValue = value is bool b && b;
        if (invert) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>Converts double opacity (0.0-1.0) to percentage string.</summary>
public sealed class OpacityToPercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d) return $"{d * 100:F0}%";
        return "0%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && double.TryParse(s.TrimEnd('%'), out var d))
            return d / 100.0;
        return 0.5;
    }
}

/// <summary>Converts bool to "Locked"/"Unlocked" text.</summary>
public sealed class LockStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? "🔒 Locked" : "🔓 Unlocked";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Converts bool to "Click-Through: ON/OFF" text.</summary>
public sealed class ClickThroughStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? "Click-Through: ON" : "Click-Through: OFF";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
