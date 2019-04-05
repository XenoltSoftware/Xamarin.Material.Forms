using System;
using Xamarin.Forms;

namespace Xamarin.Material.Forms
{
    public class TextField : Entry
    {
        public static readonly BindableProperty BorderTypeProperty = BindableProperty
            .Create(nameof(BorderType), typeof(TextFieldBorderType), typeof(TextField), TextFieldBorderType.Filled);

        public TextFieldBorderType BorderType
        {
            get => (TextFieldBorderType)GetValue(BorderTypeProperty);
            set => SetValue(BorderTypeProperty, value);
        }

        public static readonly BindableProperty FloatingEnabledProperty = BindableProperty
                    .Create(nameof(FloatingEnabled), typeof(bool), typeof(TextField), true);

        public bool FloatingEnabled
        {
            get => (bool)GetValue(FloatingEnabledProperty);
            set => SetValue(FloatingEnabledProperty, value);
        }

        public static readonly BindableProperty CharacterCountMaxProperty = BindableProperty
                    .Create(nameof(CharacterCountMax), typeof(int?), typeof(TextField));

        public int? CharacterCountMax
        {
            get => (int?)GetValue(CharacterCountMaxProperty);
            set => SetValue(CharacterCountMaxProperty, value);
        }

        public static readonly BindableProperty HelperTextProperty = BindableProperty
                    .Create(nameof(HelperText), typeof(string), typeof(TextField));

        public string HelperText
        {
            get => (string)GetValue(HelperTextProperty);
            set => SetValue(HelperTextProperty, value);
        }

        public static readonly BindableProperty ErrorProperty = BindableProperty
                    .Create(nameof(Error), typeof(string), typeof(TextField));

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public static readonly BindableProperty LeadingIconProperty = BindableProperty
                    .Create(nameof(LeadingIcon), typeof(ImageSource), typeof(TextField));

        public ImageSource LeadingIcon
        {
            get => (ImageSource)GetValue(LeadingIconProperty);
            set => SetValue(LeadingIconProperty, value);
        }

        public static readonly BindableProperty TrailingIconProperty = BindableProperty
                    .Create(nameof(TrailingIcon), typeof(ImageSource), typeof(TextField));

        public ImageSource TrailingIcon
        {
            get => (ImageSource)GetValue(TrailingIconProperty);
            set => SetValue(TrailingIconProperty, value);
        }
    }

    public enum TextFieldBorderType
    {
        Underline,
        Filled,
        Outline
    }
}
