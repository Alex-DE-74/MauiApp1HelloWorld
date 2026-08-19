using Microsoft.Maui.Controls;

namespace KidJumpUp.Behaviors;

public sealed class FocusOnTrueBehavior : Behavior<VisualElement>
{
    public static readonly BindableProperty TriggerProperty =
        BindableProperty.Create(
            nameof(Trigger),
            typeof(bool),
            typeof(FocusOnTrueBehavior),
            false,
            propertyChanged: OnTriggerChanged);

    public bool Trigger
    {
        get => (bool)GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    private VisualElement? AssociatedObject { get; set; }

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        AssociatedObject = bindable;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        AssociatedObject = null;
        base.OnDetachingFrom(bindable);
    }

    private static void OnTriggerChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is not FocusOnTrueBehavior behavior ||
            behavior.AssociatedObject is not VisualElement element ||
            newValue is not true)
        {
            return;
        }

        element.Focus();
    }
}
