using System.ComponentModel;
using System.Runtime.CompilerServices;
using KidJumpUp.Models;

namespace KidJumpUp.Models;

public class ExercisePlanItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private int? _target;

    public Exercise Exercise { get; init; } = null!;

    public int ExerciseId => Exercise.Id;

    public string Name => Exercise.Name;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            OnPropertyChanged();
        }
    }
    public int? Target
    {
        get => _target;
        set
        {
            if (_target == value)
                return;

            _target = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(TargetText));
        }
    }

    public string TargetText =>
        Target?.ToString() ?? string.Empty;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
