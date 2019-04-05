using System;
using UIKit;
using Xamarin.Forms;
using TNativeView = UIKit.UIView;

namespace Xamarin.Material.Forms.iOS.Helpers
{
    public static class FormsHelper
    {
        static bool? s_isiOS11OrNewer;

        internal static bool IsiOS11OrNewer
        {
            get
            {
                if (!s_isiOS11OrNewer.HasValue)
                    s_isiOS11OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(11, 0);
                return s_isiOS11OrNewer.Value;
            }
        }

    }
}
