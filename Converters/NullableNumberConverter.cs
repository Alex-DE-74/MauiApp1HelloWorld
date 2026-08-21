#nullable enable
using System.Globalization;
using Microsoft.Maui.Controls;

namespace KidJumpUp.Converters;

public abstract class NullableNumberConverter<T> : IValueConverter
    where T : struct, IParsable<T>, IFormattable
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is T number
            ? number.ToString(null, culture)
            : string.Empty;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return null;

        return T.TryParse(text, culture, out var result)
            ? result
            : BindableProperty.UnsetValue;
    }
}
