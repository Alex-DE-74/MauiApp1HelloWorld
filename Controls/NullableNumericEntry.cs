#nullable enable
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Handlers;

using KidJumpUp.Converters;
namespace KidJumpUp.Controls;

public class NullableNumericEntry<TValue, TConverter> : Entry
    where TValue : struct, IParsable<TValue>, IFormattable
    where TConverter : NullableNumberConverter<TValue>, new()
{
    protected override void OnHandlerChanged()
{
    base.OnHandlerChanged();

    #if ANDROID
    try
    {
        /*
        if (Handler?.PlatformView is Android.Widget.EditText nativeEdit)
        {
            // 1. Platzhalter-Text setzen
            if (!string.IsNullOrEmpty(Placeholder))
            {
                nativeEdit.Hint = Placeholder;
            }

            // 2. Platzhalter-Farbe nur setzen, wenn du eine explizit definiert hast
            if (PlaceholderColor != Colors.Transparent)
            {
                nativeEdit.SetHintTextColor(PlaceholderColor.ToPlatform());
            }

            // 3. Normale Textfarbe erzwingen, damit eingegebene Zahlen kräftig bleiben
            if (TextColor != Colors.Transparent)
            {
                nativeEdit.SetTextColor(TextColor.ToPlatform());
            }
        }
        */
    }
    catch (Exception ex)
    {
        var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Android.App.Application.Context;
        if (context != null)
        {
            Android.Widget.Toast.MakeText(context, $"Fehler: {ex.Message}", Android.Widget.ToastLength.Short)?.Show();
        }
    }
    #endif
}
        
    private static readonly TConverter Converter = new();
    private static readonly EqualityComparer<TValue?> Comparer
        = EqualityComparer<TValue?>.Default;

    // Bei Value-Änderungen den OnValueChanged unterdrücken und somit nicht mehr auf Text anwenden.
    private bool _suppressValueChanged;

    public static readonly BindableProperty IsInputNonEmptyProperty =
    BindableProperty.Create(
        nameof(IsInputNonEmpty),
        typeof(bool),
        typeof(NullableNumericEntry<TValue, TConverter>),
        false);

    public bool IsInputNonEmpty
    {
        get => (bool)GetValue(IsInputNonEmptyProperty);
        private set => SetValue(IsInputNonEmptyProperty, value);
    }

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
/*
        // Greift ein, sobald das native Android-Gegenstück bereitsteht
        HandlerChanged += (sender, args) =>
        {
            if (Handler is EntryHandler entryHandler)
            {
                #if ANDROID
var nativeEditText = entryHandler.PlatformView;
if (nativeEditText != null)
{
    // 1. Platzhalter-Text setzen
    nativeEditText.Hint = Placeholder;
    
    // 2. Platzhalter-Farbe
    if (PlaceholderColor != Colors.Transparent)
    {
        nativeEditText.SetHintTextColor(PlaceholderColor.ToPlatform());
    }

    // 3. Platzhalter-Schriftgröße (Falls gewünscht)
    // Android erwartet hier normalerweise Pixel oder Sp, deshalb rechnet man FontSize in SP um:
    if (FontSize > 0)
    {
        // Alternativ kannst du hier die Schriftgröße des Platzhalters steuern, 
        // falls Android sie fehlerhaft übernimmt.
    }
}
#endif

            }
        };
*/
        
/*
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
*/
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

        // Wenn newValue null ist, setzen wir entry.Text auf null statt auf string.Empty!
        if (newValue == null)
        {
            if (entry.Text != null)
                entry.Text = null;
        }
        else
        {
        var text =
            Converter.Convert(
                newValue,
                typeof(string),
                null,
                System.Globalization.CultureInfo.CurrentCulture)
            as string
            ?? string.Empty;

        if (entry.Text != text)
            entry.Text = text;
        }
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
            SetValue(ValueProperty, value);
        }
        finally
        {
            _suppressValueChanged = false;
        }
    }

    private void UpdateValidationState(string? text)
    {
        // 1. Prüfen, ob überhaupt Text da ist
        IsInputNonEmpty = !string.IsNullOrWhiteSpace(text);

        var converted =
            Converter.ConvertBack(
                text,
                typeof(TValue?),
                null,
                System.Globalization.CultureInfo.CurrentCulture);

        IsInputInvalid =
            IsInputNonEmpty &&
            converted == BindableProperty.UnsetValue;
    }
}
