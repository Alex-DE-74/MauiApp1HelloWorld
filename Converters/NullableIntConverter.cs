using System.Globalization;

namespace KidJumpUp.Converters;

public class NullableIntConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is int intValue)
            return intValue.ToString(culture);

        return string.Empty;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not string text ||
            string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (int.TryParse(
                text,
                NumberStyles.Integer,
                culture,
                out var result))
        {
            return result;
        }

        // Ungültige Eingabe:
        // aktuellen Wert nicht zerstören.
        return BindableProperty.UnsetValue;
    }
}
