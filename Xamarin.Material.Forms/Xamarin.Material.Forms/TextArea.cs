using System;
using Xamarin.Forms;

namespace Xamarin.Material.Forms
{
    public class TextArea : Editor
    {
        public TextArea()
        {
        }

        public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(Editor), true, BindingMode.Default);

        public bool IsTextPredictionEnabled
        {
            get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
            set { SetValue(IsTextPredictionEnabledProperty, value); }
        }
    }
}
