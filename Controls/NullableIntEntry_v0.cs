using System.Globalization;
using KidJumpUp.Converters;
using Microsoft.Maui.Controls;

namespace KidJumpUp.Controls;

public class NullableIntEntry_v0 : Entry
{
    private static readonly NullableIntConverter Converter = new();

    private Color? _normalTextColor;

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(
            nameof(Value),
            typeof(int?),
            typeof(NullableIntEntry_v0),
            default(int?),
            BindingMode.TwoWay,
            propertyChanged: OnValueChanged);

    public static readonly BindableProperty InvalidTextColorProperty =
        BindableProperty.Create(
            nameof(InvalidTextColor),
            typeof(Color),
            typeof(NullableIntEntry_v0),
            Colors.Red);

    public int? Value
    {
        get => (int?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color InvalidTextColor
    {
        get => (Color)GetValue(InvalidTextColorProperty);
        set => SetValue(InvalidTextColorProperty, value);
    }

    public NullableIntEntry_v0()
    {
        Keyboard = Keyboard.Numeric;

        _normalTextColor = TextColor;

        TextChanged += OnTextChanged;
    }

    private static void OnValueChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is not NullableIntEntry_v0 entry)
            return;

        var text =
            Converter.Convert(
                newValue,
                typeof(string),
                null,
                CultureInfo.CurrentCulture)
            as string
            ?? string.Empty;

        if (entry.Text != text)
            entry.Text = text;

        entry.UpdateValidationVisualState(
            entry.Text);
    }

    private void OnTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        UpdateValidationVisualState(
            e.NewTextValue);

        var converted =
            Converter.ConvertBack(
                e.NewTextValue,
                typeof(int?),
                null,
                CultureInfo.CurrentCulture);

        if (converted == BindableProperty.UnsetValue)
            return;

        int? value = converted switch
        {
            int intValue => intValue,
            null => null,
            _ => null
        };

        if (Value != value)
            Value = value;
    }

    private void UpdateValidationVisualState(
        string? text)
    {
        var isValid =
            string.IsNullOrWhiteSpace(text) ||
            int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.CurrentCulture,
                out _);

        if (isValid)
        {
            TextColor = _normalTextColor;
        }
        else
        {
            if (_normalTextColor == null)
                _normalTextColor = TextColor;

            TextColor = InvalidTextColor;
        }
    }
}
