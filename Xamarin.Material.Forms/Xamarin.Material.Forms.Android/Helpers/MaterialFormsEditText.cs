using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Material.Forms.Android
{
    public class MaterialFormsEditText : TextInputEditText
    {
        //DescendantFocusToggler _descendantFocusToggler;

        // These paddings are a hack to center the hint
        // once this issue is resolved we can get rid of these paddings
        // https://github.com/material-components/material-components-android/issues/120
        // https://stackoverflow.com/questions/50487871/how-to-make-the-hint-text-of-textinputlayout-vertically-center

        static Thickness _centeredText = new Thickness(12, 8, 12, 27);
        static Thickness _alignedWithUnderlineText = new Thickness(12, 20, 12, 16);

        public MaterialFormsEditText(Context context) : base(context)
        {
        }

        void UpdatePadding()
        {
            Thickness rect = _centeredText;

            if (!String.IsNullOrWhiteSpace(Text) || HasFocus)
            {
                rect = _alignedWithUnderlineText;
            }

            SetPadding((int)Context.ToPixels(rect.Left), (int)Context.ToPixels(rect.Top), (int)Context.ToPixels(rect.Right), (int)Context.ToPixels(rect.Bottom));
        }

        protected override void OnTextChanged(Java.Lang.ICharSequence text, int start, int lengthBefore, int lengthAfter)
        {
            base.OnTextChanged(text, start, lengthBefore, lengthAfter);
            if (lengthBefore == 0 || lengthAfter == 0)
                UpdatePadding();
        }

        protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);

            // Delay padding update until after the keyboard has showed up otherwise updating the padding
            // stops the keyboard from showing up
            if (gainFocus)
                Device.BeginInvokeOnMainThread(() => UpdatePadding());
            else
                UpdatePadding();
        }

        //bool IDescendantFocusToggler.RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus)
        //{
        //    _descendantFocusToggler = _descendantFocusToggler ?? new DescendantFocusToggler();

        //    return _descendantFocusToggler.RequestFocus(control, baseRequestFocus);
        //}

        public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
        {
            if (keyCode != Keycode.Back || e.Action != KeyEventActions.Down)
            {
                return base.OnKeyPreIme(keyCode, e);
            }

            this.HideKeyboard();

            _onKeyboardBackPressed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //public override bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect)
        //{
        //    return (this as IDescendantFocusToggler).RequestFocus(this, () => base.RequestFocus(direction, previouslyFocusedRect));
        //}

        protected override void OnSelectionChanged(int selStart, int selEnd)
        {
            base.OnSelectionChanged(selStart, selEnd);
            _selectionChanged?.Invoke(this, new SelectionChangedEventArgs(selStart, selEnd));
        }

        event EventHandler _onKeyboardBackPressed;
        public event EventHandler OnKeyboardBackPressed
        {
            add => _onKeyboardBackPressed += value;
            remove => _onKeyboardBackPressed -= value;
        }

        event EventHandler<SelectionChangedEventArgs> _selectionChanged;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged
        {
            add => _selectionChanged += value;
            remove => _selectionChanged -= value;
        }
    }
}
