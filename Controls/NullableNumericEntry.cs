using Microsoft.Maui.Controls;
using KidJumpUp.Converters;

namespace KidJumpUp.Controls;

public class NullableNumericEntry<TValue, TConverter> : Entry
    where TValue : struct, IParsable<TValue>, IFormattable
    where TConverter : NullableNumberConverter<TValue>, new()
{
    private static readonly TConverter Converter = new();
    private static readonly EqualityComparer<TValue?> Comparer
        = EqualityComparer<TValue?>.Default;

    // Bei Value-Änderungen den OnValueChanged unterdrücken und somit nicht mehr auf Text anwenden.
    private bool _suppressValueChanged;

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(
            nameof(Value),
            typeof(TValue?),
            typeof(NullableNumericEntry<TValue, TConverter>),
            default(TValue?),
            BindingMode.TwoWay,
            propertyChanged: OnValueChanged);

    public static readonly BindableProperty InvalidTextColorProperty =
        BindableProperty.Create(
            nameof(InvalidTextColor),
            typeof(Color),
            typeof(NullableNumericEntry<TValue, TConverter>),
            Colors.Red);

    public static readonly BindableProperty IsInputInvalidProperty =
        BindableProperty.Create(
            nameof(IsInputInvalid),
            typeof(bool),
            typeof(NullableNumericEntry<TValue, TConverter>),
            false);

    public TValue? Value
    {
        get => (TValue?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color InvalidTextColor
    {
        get => (Color)GetValue(InvalidTextColorProperty);
        set => SetValue(InvalidTextColorProperty, value);
    }

    public bool IsInputInvalid
    {
        get => (bool)GetValue(IsInputInvalidProperty);
        private set => SetValue(IsInputInvalidProperty, value);
    }

    public NullableNumericEntry()
    {
        Keyboard = Keyboard.Numeric;

        Triggers.Add(
            new DataTrigger(typeof(NullableNumericEntry<TValue, TConverter>))
            {
                Binding = new Binding(
                    nameof(IsInputInvalid),
                    source: this),

                Value = true,

                Setters =
                {
                    new Setter
                    {
                        Property = TextColorProperty,
                        Value = new Binding(
                            nameof(InvalidTextColor),
                            source: this)
                    }
                }
            });

        TextChanged += OnTextChanged;
    }

    private static void OnValueChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is not NullableNumericEntry<TValue, TConverter> entry)
            return;

        // Rückkopplung während der TextProperty-Änderung vermeiden.
        if (entry._suppressValueChanged) return;
        
        var text =
            Converter.Convert(
                newValue,
                typeof(string),
                null,
                System.Globalization.CultureInfo.CurrentCulture)
            as string
            ?? string.Empty;

        // if (entry.Text != text)
            entry.Text = text;

        entry.UpdateValidationState(entry.Text);
    }

    private void OnTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        var converted =
            Converter.ConvertBack(
                e.NewTextValue,
                typeof(TValue?),
                null,
                System.Globalization.CultureInfo.CurrentCulture);

        IsInputInvalid =
            converted == BindableProperty.UnsetValue;

        if (IsInputInvalid)
            return;

        TValue? value = converted switch
        {
            TValue parsed => parsed,
            null => null,
            _ => null
        };

        // Wenn unterschiedlich, dann ohne Rückkopplung setzen.
        if (!Comparer.Equals(Value, value))
            SetValueSilently(value);
    }

    private void SetValueSilently(TValue? value)
    {
        _suppressValueChanged = true;

        try
        {
            // Bindung erhalten
            SetCurrentValue(ValueProperty, value);
        }
        finally
        {
            _suppressValueChanged = false;
        }
    }

    private void UpdateValidationState(string? text)
    {
        var converted =
            Converter.ConvertBack(
                text,
                typeof(TValue?),
                null,
                System.Globalization.CultureInfo.CurrentCulture);

        IsInputInvalid =
            converted == BindableProperty.UnsetValue;
    }
}
