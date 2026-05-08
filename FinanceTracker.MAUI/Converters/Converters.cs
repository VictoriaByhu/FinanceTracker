using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FinanceTracker.MAUI.Converters;

/// <summary>
/// true (income) → "#1D9E75" | false (expense) → "#E24B4A"
/// </summary>
public class BoolToAmountColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isIncome = value is bool b && b;
        return isIncome ? Color.FromArgb("#1D9E75") : Color.FromArgb("#E24B4A");
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// true (income) → rgba(29,158,117,0.15) | false → rgba(226,75,74,0.15)
/// </summary>
public class BoolToBadgeBgConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isIncome = value is bool b && b;
        return isIncome
            ? Color.FromArgb("#1A3D2E")
            : Color.FromArgb("#3D1A1A");
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// true (income) → background tint green | false → red (for icon background)
/// </summary>
public class BoolToIncomeColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isIncome = value is bool b && b;
        return isIncome
            ? Color.FromArgb("#1A3D2E")
            : Color.FromArgb("#3D1A1A");
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// true (income) → "💰" | false (expense) → "💸"
/// </summary>
public class BoolToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isIncome = value is bool b && b;
        return isIncome ? "💰" : "💸";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// true (income) → "+₴{amount}" | false → "−₴{amount}"
/// Used with Amount value, IsIncome passed via parameter (not ideal — see ViewModel AmountFormatted instead)
/// </summary>
public class AmountSignConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not decimal amount) return "₴0";
        bool isIncome = parameter is bool b && b;
        string sign = isIncome ? "+" : "−";
        return $"{sign}₴{amount:N0}";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// "#1D9E75" (string) → Color
/// Falls back to "#607D8B" if parse fails.
/// </summary>
public class StringToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try { return Color.FromArgb(hex); }
            catch { /* fall through */ }
        }
        return Color.FromArgb("#607D8B");
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}