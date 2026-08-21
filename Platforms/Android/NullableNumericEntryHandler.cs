#if ANDROID
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AndroidX.AppCompat.Widget;

namespace DeinNamespace.Platforms.Android
{
    public class NullableNumericEntryHandler : EntryHandler
    {
        protected override MauiAppCompatEditText CreatePlatformView()
        {
            var nativeView = base.CreatePlatformView();
            // Hier greifst du absolut sicher auf das fertige Android-Control zu, 
            // ohne dass es beim Initialisieren abstürzt.
            return nativeView;
        }

        protected override void ConnectHandler(MauiAppCompatEditText platformView)
        {
            base.ConnectHandler(platformView);
            // Platzhalter-Fix direkt beim Verbinden des Handlers
            if (VirtualView is Entry entry && !string.IsNullOrEmpty(entry.Placeholder))
            {
                platformView.Hint = entry.Placeholder;
                if (entry.PlaceholderColor != Colors.Transparent)
                {
                    platformView.SetHintTextColor(entry.PlaceholderColor.ToPlatform());
                }
            }
        }
    }
}
#endif
