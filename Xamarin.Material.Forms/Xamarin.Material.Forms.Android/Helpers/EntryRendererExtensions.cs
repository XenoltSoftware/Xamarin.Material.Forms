using System;
using Android.Views.InputMethods;
using Xamarin.Forms;

namespace Xamarin.Material.Forms.Android
{
    internal static class EntryRendererExtensions
    {
        internal static ImeAction ToAndroidImeAction(this ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.Go:
                    return ImeAction.Go;
                case ReturnType.Next:
                    return ImeAction.Next;
                case ReturnType.Send:
                    return ImeAction.Send;
                case ReturnType.Search:
                    return ImeAction.Search;
                case ReturnType.Done:
                    return ImeAction.Done;
                case ReturnType.Default:
                    return ImeAction.Done;
                default:
                    throw new System.NotImplementedException($"ReturnType {returnType} not supported");
            }
        }

        public static ImeAction ToAndroidImeOptions(this Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags flags)
        {
            switch (flags)
            {
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.Previous:
                    return ImeAction.Previous;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.Next:
                    return ImeAction.Next;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.Search:
                    return ImeAction.Search;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.Send:
                    return ImeAction.Send;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.Go:
                    return ImeAction.Go;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.None:
                    return ImeAction.None;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.ImeMaskAction:
                    return ImeAction.ImeMaskAction;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.NoPersonalizedLearning:
                    return (ImeAction)ImeFlags.NoPersonalizedLearning;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.NoExtractUi:
                    return (ImeAction)ImeFlags.NoExtractUi;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.NoAccessoryAction:
                    return (ImeAction)ImeFlags.NoAccessoryAction;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.NoFullscreen:
                    return (ImeAction)ImeFlags.NoFullscreen;
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.Default:
                case Xamarin.Forms.PlatformConfiguration.AndroidSpecific.ImeFlags.Done:
                default:
                    return ImeAction.Done;
            }
        }
    }
}
