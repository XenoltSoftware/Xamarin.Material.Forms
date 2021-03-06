﻿using System;
using System.Collections.Generic;
using Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using AApplication = Android.App.Application;

namespace Xamarin.Material.Forms.Android
{
    public static class FontExtensions
    {
        static readonly Dictionary<Tuple<string, FontAttributes>, Typeface> Typefaces = new Dictionary<Tuple<string, FontAttributes>, Typeface>();

        static Typeface s_defaultTypeface;

        public static Typeface ToTypeface(this Font self)
        {
            if (self.IsDefault || (self.FontAttributes == FontAttributes.None && string.IsNullOrEmpty(self.FontFamily)))
                return s_defaultTypeface ?? (s_defaultTypeface = Typeface.Default);

            var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
            Typeface result;
            if (Typefaces.TryGetValue(key, out result))
                return result;

            if (self.FontFamily == null)
            {
                var style = ToTypefaceStyle(self.FontAttributes);
                result = Typeface.Create(Typeface.Default, style);
            }
            else if (IsAssetFontFamily(self.FontFamily))
            {
                result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(self.FontFamily));
            }
            else
            {
                var style = ToTypefaceStyle(self.FontAttributes);
                result = Typeface.Create(self.FontFamily, style);
            }
            return (Typefaces[key] = result);
        }

        internal static bool IsDefault(this IFontElement self)
        {
            return self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;
        }

        internal static Typeface ToTypeface(this IFontElement self)
        {
            if (self.IsDefault())
                return s_defaultTypeface ?? (s_defaultTypeface = Typeface.Default);

            var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
            Typeface result;
            if (Typefaces.TryGetValue(key, out result))
                return result;

            if (self.FontFamily == null)
            {
                var style = ToTypefaceStyle(self.FontAttributes);
                result = Typeface.Create(Typeface.Default, style);
            }
            else if (IsAssetFontFamily(self.FontFamily))
            {
                result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(self.FontFamily));
            }
            else
            {
                var style = ToTypefaceStyle(self.FontAttributes);
                result = Typeface.Create(self.FontFamily, style);
            }
            return (Typefaces[key] = result);
        }

        public static TypefaceStyle ToTypefaceStyle(FontAttributes attrs)
        {
            var style = TypefaceStyle.Normal;
            if ((attrs & (FontAttributes.Bold | FontAttributes.Italic)) == (FontAttributes.Bold | FontAttributes.Italic))
                style = TypefaceStyle.BoldItalic;
            else if ((attrs & FontAttributes.Bold) != 0)
                style = TypefaceStyle.Bold;
            else if ((attrs & FontAttributes.Italic) != 0)
                style = TypefaceStyle.Italic;
            return style;
        }

        static bool IsAssetFontFamily(string name)
        {
            return name.Contains(".ttf#") || name.Contains(".otf#");
        }

        static string FontNameToFontFile(string fontFamily)
        {
            int hashtagIndex = fontFamily.IndexOf('#');
            if (hashtagIndex >= 0)
                return fontFamily.Substring(0, hashtagIndex);

            throw new InvalidOperationException($"Can't parse the {nameof(fontFamily)} {fontFamily}");
        }
    }
}
