using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Material.Forms.Android
{
    public static class VisualElementExtensions
    {
        internal static bool UseLegacyColorManagement<T>(this T element) where T : Xamarin.Forms.VisualElement, IElementConfiguration<T>
        {
            // Determine whether we're letting the VSM handle the colors or doing it the old way
            // or disabling the legacy color management and doing it the old-old (pre 2.0) way
            return !element.HasVisualStateGroups()
                    && element.OnThisPlatform().GetIsLegacyColorModeEnabled();
        }
    }
}
