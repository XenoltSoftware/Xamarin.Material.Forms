using System;
using Android.OS;
using Android.Widget;
using Xamarin.Forms.Internals;
using ALayoutDirection = Android.Views.LayoutDirection;
using AView = Android.Views.View;
using AGravityFlags = Android.Views.GravityFlags;
using Xamarin.Forms;
using Android.Views;
using ATextAlignment = Android.Views.TextAlignment;

namespace Xamarin.Material.Forms.Android
{
    internal static class FlowDirectionExtensions
    {
        //internal static FlowDirection ToFlowDirection(this LayoutDirection direction)
        //{
        //    switch (direction)
        //    {
        //        case LayoutDirection.Ltr:
        //            return FlowDirection.LeftToRight;
        //        case ALayoutDirection.Rtl:
        //            return FlowDirection.RightToLeft;
        //        default:
        //            return FlowDirection.MatchParent;
        //    }
        //}

        //internal static void UpdateFlowDirection(this View view, IVisualElementController controller)
        //{
        //    if (view == null || controller == null || (int)Build.VERSION.SdkInt < 17)
        //        return;

        //    // if android:targetSdkVersion < 17 setting these has no effect
        //    if (controller.EffectiveFlowDirection.IsRightToLeft())
        //        view.LayoutDirection = ALayoutDirection.Rtl;
        //    else if (controller.EffectiveFlowDirection.IsLeftToRight())
        //        view.LayoutDirection = ALayoutDirection.Ltr;
        //}

        internal static void UpdateHorizontalAlignment(this EditText view, Xamarin.Forms.TextAlignment alignment, bool hasRtlSupport, AGravityFlags orMask = AGravityFlags.NoGravity)
        {
            if ((int)Build.VERSION.SdkInt < 17 || !hasRtlSupport)
                view.Gravity = alignment.ToHorizontalGravityFlags() | orMask;
            else
                view.TextAlignment = alignment.ToTextAlignment();
        }

        internal static GravityFlags ToHorizontalGravityFlags(this Xamarin.Forms.TextAlignment alignment)
        {
            switch (alignment)
            {
                case Xamarin.Forms.TextAlignment.Center:
                    return GravityFlags.CenterHorizontal;
                case Xamarin.Forms.TextAlignment.End:
                    return GravityFlags.End;
                default:
                    return GravityFlags.Start;
            }
        }

        internal static ATextAlignment ToTextAlignment(this Xamarin.Forms.TextAlignment alignment)
        {
            switch (alignment)
            {
                case Xamarin.Forms.TextAlignment.Center:
                    return ATextAlignment.Center;
                case Xamarin.Forms.TextAlignment.End:
                    return ATextAlignment.ViewEnd;
                default:
                    return ATextAlignment.ViewStart;
            }
        }
    }
}
