using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace KidJumpUp.Converters;

public class NullableIntValue : INotifyPropertyChanged
{
    private int? _value;

    public int? Value
    {
        get => _value;
        set
        {
            if (_value == value)
                return;

            _value = value;
            OnPropertyChanged();
        }
    }

    public string Text
    {
        get => _value?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Value = null;
                return;
            }

            if (int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var result))
            {
                Value = result;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
