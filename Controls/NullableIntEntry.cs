using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

#if ANDROID
using AndroidX.AppCompat.Widget;
#endif

using KidJumpUp.Converters;

namespace KidJumpUp.Controls;

public class NullableIntEntry
    : NullableNumericEntry<int, NullableIntConverter>
{
/*
/) Funktioniert so nicht!
    // Statischer Konstruktor läuft einmalig beim ersten Laden der Klasse
    static NullableIntEntry()
    {
        // Überschreibt/Erweitert das Mapping global für diesen Typ, 
        // ohne dass du jemals die MauiProgram.cs anfassen musst!
        EntryHandler.Mapper.AppendToMapping(nameof(NullableIntEntry), (handler, view) =>
        {
            if (view is NullableIntEntry)
            {
                #if ANDROID
                if (handler.PlatformView is AppCompatEditText nativeEditText)
                {
                    nativeEditText.Hint = view.Placeholder;
                    if (view.PlaceholderColor != Colors.Transparent)
                    {
                        nativeEditText.SetHintTextColor(view.PlaceholderColor.ToPlatform());
                    }
                }
                #endif
            }
        });
    }
*/
}
