public abstract class NullableNumberConverter<T> : IValueConverter
    where T : struct, IParsable<T>
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => value is T number
            ? number.ToString(null, culture)
            : string.Empty;

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => value is not string text || string.IsNullOrWhiteSpace(text)
            ? null
            : T.TryParse(text, culture, out var result)
                ? result
                : BindableProperty.UnsetValue;
}
