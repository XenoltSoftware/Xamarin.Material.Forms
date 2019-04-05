using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Material.Forms;
using TextAlignment = Android.Views.TextAlignment;
using InputTypes = Android.Text.InputTypes;
using Android.Text;
using Android.Widget;
using Android.Views.InputMethods;
using Java.Lang;
using System.ComponentModel;
using Android.Text.Method;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Util;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Android.Support.V4.Content;
using Android.Graphics.Drawables;

[assembly: ExportRenderer(typeof(TextField), typeof(Xamarin.Material.Forms.Android.Renderers.TextFieldRenderer))]
namespace Xamarin.Material.Forms.Android.Renderers
{
    public class TextFieldRenderer : ViewRenderer<TextField, TextInputLayout>, ITextWatcher, TextView.IOnEditorActionListener
    {
        TextColorSwitcher _hintColorSwitcher;
        TextColorSwitcher _textColorSwitcher;
        internal bool HandleKeyboardOnFocus;

        bool _disposed;
        ImeAction _currentInputImeFlag;
        IElementController ElementController => Element as IElementController;

        bool _cursorPositionChangePending;
        bool _selectionLengthChangePending;
        bool _nativeSelectionIsUpdating;

        private MaterialFormsEditText _textInputEditText;

        protected MaterialFormsEditText EditText => _textInputEditText;

        public TextFieldRenderer(Context context) : base(context)
        {
            AutoPackage = false;
        }

        bool TextView.IOnEditorActionListener.OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
        {
            // Fire Completed and dismiss keyboard for hardware / physical keyboards
            if (actionId == ImeAction.Done || actionId == _currentInputImeFlag || (actionId == ImeAction.ImeNull && e.KeyCode == Keycode.Enter && e.Action == KeyEventActions.Up))
            {
                Control.ClearFocus();
                v.HideKeyboard(); 
                ((IEntryController)Element).SendCompleted();
            }

            return true;
        }


        protected override void OnElementChanged(ElementChangedEventArgs<TextField> e)
        {
            base.OnElementChanged(e);
            HandleKeyboardOnFocus = true;

            if(e.OldElement == null)
            {
                var textView = CreateNativeControl();
                SetNativeControl(textView);
                EditText.AddTextChangedListener(this);
                EditText.SetOnEditorActionListener(this);
                EditText.OnKeyboardBackPressed += OnKeyboardBackPressed;
                EditText.SelectionChanged += SelectionChanged;
                //ToDo
                //var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement(e);
                var useLegacyColorManagement = true;

                _textColorSwitcher = new TextColorSwitcher(EditText.TextColors, useLegacyColorManagement);
                _hintColorSwitcher = new TextColorSwitcher(EditText.HintTextColors, useLegacyColorManagement);
                SetNativeControl(textView);
            }

            // When we set the control text, it triggers the SelectionChanged event, which updates CursorPosition and SelectionLength;
            // These one-time-use variables will let us initialize a CursorPosition and SelectionLength via ctor/xaml/etc.
            _cursorPositionChangePending = Element.IsSet(TextField.CursorPositionProperty);
            _selectionLengthChangePending = Element.IsSet(TextField.SelectionLengthProperty);

            UpdatePlaceHolderText();

            EditText.Text = Element.Text;
            UpdateInputType();

            UpdateColor();
            UpdateAlignment();
            UpdateFont();
            UpdatePlaceholderColor();
            UpdateMaxLength();
            UpdateImeOptions();
            UpdateReturnType();
            UpdateFloatingEnabled();
            UpdateHelperText();
            UpdateErrorText();
            UpdateCharacterCountMax();
            UpdateLeadingIcon();
            UpdateTrailingIcon();

            if (_cursorPositionChangePending || _selectionLengthChangePending)
                UpdateCursorSelection();
        }


        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                if (EditText != null)
                {
                    EditText.OnKeyboardBackPressed -= OnKeyboardBackPressed;
                    EditText.SelectionChanged -= SelectionChanged;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TextField.PlaceholderProperty.PropertyName)
                UpdatePlaceHolderText();
            else if (e.PropertyName == TextField.IsPasswordProperty.PropertyName)
                UpdateInputType();
            else if (e.PropertyName == TextField.TextProperty.PropertyName)
            {
                if (EditText.Text != Element.Text)
                {
                    EditText.Text = Element.Text;
                    if (Control.IsFocused)
                    {
                        EditText.SetSelection(EditText.Text.Length);
                        Control.ShowKeyboard();
                    }
                }
            }
            else if (e.PropertyName == TextField.TextColorProperty.PropertyName)
                UpdateColor();
            else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
                UpdateInputType();
            else if (e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
                UpdateInputType();
            else if (e.PropertyName == TextField.IsTextPredictionEnabledProperty.PropertyName)
                UpdateInputType();
            else if (e.PropertyName == TextField.HorizontalTextAlignmentProperty.PropertyName)
                UpdateAlignment();
            else if (e.PropertyName == TextField.FontAttributesProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == TextField.FontFamilyProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == TextField.FontSizeProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == TextField.PlaceholderColorProperty.PropertyName)
                UpdatePlaceholderColor();
            else if (e.PropertyName == Xamarin.Forms.VisualElement.FlowDirectionProperty.PropertyName)
                UpdateAlignment();
            else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
                UpdateMaxLength();
            //ToDO
            //else if (e.PropertyName == PlatformConfiguration.AndroidSpecific.TextField.ImeOptionsProperty.PropertyName)
            //UpdateImeOptions();
            else if (e.PropertyName == TextField.ReturnTypeProperty.PropertyName)
                UpdateReturnType();
            else if (e.PropertyName == TextField.SelectionLengthProperty.PropertyName)
                UpdateCursorSelection();
            else if (e.PropertyName == TextField.CursorPositionProperty.PropertyName)
                UpdateCursorSelection();

            base.OnElementPropertyChanged(sender, e);
        }

        protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
        {
            // Override this in a custom renderer to use a different NumberKeyListener
            // or to filter out input types you don't want to allow
            // (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
            return LocalizedDigitsKeyListener.Create(inputTypes);
        }

        protected virtual void UpdateImeOptions()
        {
            if (Element == null || Control == null)
                return;
            //ToDo
            //var imeOptions = Element.OnThisPlatform().ImeOptions();
            //_currentInputImeFlag = imeOptions.ToAndroidImeOptions();
            //EditText.ImeOptions = _currentInputImeFlag;
        }

        void UpdateAlignment()
        {
            EditText.UpdateHorizontalAlignment(Element.HorizontalTextAlignment, Context.HasRtlSupport());
        }

        void UpdateColor()
        {
            _textColorSwitcher.UpdateTextColor(EditText, Element.TextColor);
        }

        void UpdateFont()
        {
            EditText.Typeface = Element.ToTypeface();
            EditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
        }

        void UpdateInputType()
        {
            TextField model = Element;
            var keyboard = model.Keyboard;

            EditText.InputType = keyboard.ToInputType();
            if (!(keyboard is Xamarin.Forms.Internals.CustomKeyboard))
            {
                if (model.IsSet(InputView.IsSpellCheckEnabledProperty))
                {
                    if ((EditText.InputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
                    {
                        if (!model.IsSpellCheckEnabled)
                            EditText.InputType = EditText.InputType | InputTypes.TextFlagNoSuggestions;
                    }
                }
                if (model.IsSet(TextField.IsTextPredictionEnabledProperty))
                {
                    if ((EditText.InputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
                    {
                        if (!model.IsTextPredictionEnabled)
                            EditText.InputType = EditText.InputType | InputTypes.TextFlagNoSuggestions;
                    }
                }
            }

            if (keyboard == Keyboard.Numeric)
            {
                EditText.KeyListener = GetDigitsKeyListener(EditText.InputType);
            }

            if (model.IsPassword && ((EditText.InputType & InputTypes.ClassText) == InputTypes.ClassText))
                EditText.InputType = EditText.InputType | InputTypes.TextVariationPassword;
            if (model.IsPassword && ((EditText.InputType & InputTypes.ClassNumber) == InputTypes.ClassNumber))
                EditText.InputType = EditText.InputType | InputTypes.NumberVariationPassword;

            UpdateFont();
        }

        void UpdatePlaceholderColor()
        {
            _hintColorSwitcher.UpdateTextColor(EditText, Element.PlaceholderColor, EditText.SetHintTextColor);
        }

        void OnKeyboardBackPressed(object sender, EventArgs eventArgs)
        {
            Control?.ClearFocus();
        }

        void UpdateMaxLength()
        {
            var currentFilters = new List<IInputFilter>(EditText?.GetFilters() ?? new IInputFilter[0]);

            for (var i = 0; i < currentFilters.Count; i++)
            {
                if (currentFilters[i] is InputFilterLengthFilter)
                {
                    currentFilters.RemoveAt(i);
                    break;
                }
            }

            currentFilters.Add(new InputFilterLengthFilter(Element.MaxLength));

            EditText?.SetFilters(currentFilters.ToArray());

            var currentControlText = EditText?.Text;

            if (currentControlText.Length > Element.MaxLength)
                EditText.Text = currentControlText.Substring(0, Element.MaxLength);
        }

        void UpdateReturnType()
        {
            if (Control == null || Element == null)
                return;

            EditText.ImeOptions = Element.ReturnType.ToAndroidImeAction();
            _currentInputImeFlag = EditText.ImeOptions;
        }

         void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_nativeSelectionIsUpdating || Control == null || Element == null)
                return;

            int cursorPosition = Element.CursorPosition;
            int selectionStart = EditText.SelectionStart;

            if (!_cursorPositionChangePending)
            {
                var start = cursorPosition;

                if (selectionStart != start)
                    SetCursorPositionFromRenderer(selectionStart);
            }

            if (!_selectionLengthChangePending)
            {
                int elementSelectionLength = System.Math.Min(EditText.Text.Length - cursorPosition, Element.SelectionLength);

                var controlSelectionLength = EditText.SelectionEnd - selectionStart;
                if (controlSelectionLength != elementSelectionLength)
                    SetSelectionLengthFromRenderer(controlSelectionLength);
            }
        }

        internal protected virtual void UpdatePlaceHolderText() => Control.Hint = Element.Placeholder;

        void UpdateCursorSelection()
        {
            if (_nativeSelectionIsUpdating || Control == null || Element == null)
                return;

            if (Control.RequestFocus())
            {
                try
                {
                    int start = GetSelectionStart();
                    int end = GetSelectionEnd(start);

                    EditText.SetSelection(start, end);
                }
                catch (System.Exception ex)
                {
                    Xamarin.Forms.Internals.Log.Warning("Entry", $"Failed to set Control.Selection from CursorPosition/SelectionLength: {ex}");
                }
                finally
                {
                    _cursorPositionChangePending = _selectionLengthChangePending = false;
                }
            }
        }

        int GetSelectionEnd(int start)
        {
            int end = start;
            int selectionLength = Element.SelectionLength;

            if (Element.IsSet(TextField.SelectionLengthProperty))
                end = System.Math.Max(start, System.Math.Min(EditText.Length(), start + selectionLength));

            int newSelectionLength = System.Math.Max(0, end - start);
            if (newSelectionLength != selectionLength)
                SetSelectionLengthFromRenderer(newSelectionLength);

            return end;
        }

        int GetSelectionStart()
        {
            int start = EditText.Length();
            int cursorPosition = Element.CursorPosition;

            if (Element.IsSet(TextField.CursorPositionProperty))
                start = System.Math.Min(EditText.Text.Length, cursorPosition);

            if (start != cursorPosition)
                SetCursorPositionFromRenderer(start);

            return start;
        }

        void SetCursorPositionFromRenderer(int start)
        {
            try
            {
                _nativeSelectionIsUpdating = true;
                ElementController?.SetValueFromRenderer(TextField.CursorPositionProperty, start);
            }
            catch (System.Exception ex)
            {
                Xamarin.Forms.Internals.Log.Warning("Entry", $"Failed to set CursorPosition from renderer: {ex}");
            }
            finally
            {
                _nativeSelectionIsUpdating = false;
            }
        }

        void SetSelectionLengthFromRenderer(int selectionLength)
        {
            try
            {
                _nativeSelectionIsUpdating = true;
                ElementController?.SetValueFromRenderer(TextField.SelectionLengthProperty, selectionLength);
            }
            catch (System.Exception ex)
            {
                Xamarin.Forms.Internals.Log.Warning("Entry", $"Failed to set SelectionLength from renderer: {ex}");
            }
            finally
            {
                _nativeSelectionIsUpdating = false;
            }
        }

        public void AfterTextChanged(IEditable s)
        {
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            if (string.IsNullOrEmpty(Element.Text) && s.Length() == 0)
                return;

            ((IElementController)Element).SetValueFromRenderer(TextField.TextProperty, s.ToString());
        }

        protected override TextInputLayout CreateNativeControl()
        {
            var textInputLayout = GetNativeControl();
            //textInputLayout.HintEnabled = true;
            //textInputLayout.CounterEnabled = true;
            //textInputLayout.CounterMaxLength = 40;
            //textInputLayout.PasswordVisibilityToggleEnabled = true;
            //textInputLayout.HelperText = "Required";
            //textInputLayout.Error = "Error";

            _textInputEditText = new MaterialFormsEditText(textInputLayout.Context)
            {
                //SupportBackgroundTintList = ColorStateList.ValueOf(GetPlaceholderColor())
            };
            //_textInputEditText.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.clear, 0, Resource.Drawable.clear, 0);
            //_textInputEditText.Hint = "Username1";
            //_textInputEditText.Text = "Test1";
            //_textInputEditText.SetMaxLines(1);
            //_textInputEditText.InputType = InputTypes.ClassText | InputTypes.TextVariationPassword;

            textInputLayout.AddView(_textInputEditText);
            return textInputLayout;
        }

        private TextInputLayout GetNativeControl()
        {
            if (Element.BorderType == TextFieldBorderType.Filled)
            {
                return GetFilledTextInputLayout();
            }
            else if (Element.BorderType == TextFieldBorderType.Outline)
            {
                return GetOutlineTextInputLayout();
            }
            else
            {
                return GetDefaultTextInputLayout();
            }
        }

        private void UpdateFloatingEnabled()
        {
            Control.HintEnabled = Element.FloatingEnabled;
        }

        private void UpdateHelperText()
        {
            Control.HelperText = Element.HelperText;
        }

        private void UpdateErrorText()
        {
            Control.Error = Element.Error;
        }

        private void UpdateCharacterCountMax()
        {
            if (Element.CharacterCountMax == null)
            {
                Control.CounterEnabled = false;
                return;
            }

            Control.CounterEnabled = true;
            Control.CounterMaxLength = Element.CharacterCountMax.Value;
        }

        private void UpdateLeadingIcon()
        {
            if (Element?.LeadingIcon == null || EditText == null)
                return;
            try
            {
                var iconId = GetImageSourceIconId(Element.LeadingIcon);
                var rightDrawable = EditText.GetCompoundDrawables()[2];
                var leftDrawable = ContextCompat.GetDrawable(Context, iconId);
                EditText.SetCompoundDrawablesWithIntrinsicBounds(leftDrawable, null, rightDrawable, null);
                EditText.CompoundDrawablePadding = 20;
            }
            catch (System.Exception ex)
            {
                //Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
            }
        }

        private void UpdateTrailingIcon()
        {
            if (Element?.TrailingIcon == null || EditText == null)
                return;
            try
            {
                var iconId = GetImageSourceIconId(Element.TrailingIcon);
                var leftDrawable = EditText.GetCompoundDrawables()[0];
                var rightDrawable = ContextCompat.GetDrawable(Context, iconId);
                EditText.SetCompoundDrawablesWithIntrinsicBounds(leftDrawable, null, rightDrawable, null);
                EditText.CompoundDrawablePadding = 20;
            }
            catch (System.Exception ex)
            {
                //Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
            }
        }

        private TextInputLayout GetFilledTextInputLayout()
        {
            return (TextInputLayout)LayoutInflater.From(Context).Inflate(Resource.Layout.FilledTextInputLayout, null);
        }

        private TextInputLayout GetOutlineTextInputLayout()
        {
            return (TextInputLayout)LayoutInflater.From(Context).Inflate(Resource.Layout.OutlineTextInputLayout, null);
        }

        private TextInputLayout GetDefaultTextInputLayout()
        {
            return new TextInputLayout(Context);
        }

        int GetImageSourceIconId(ImageSource imageSource)
        {
            var icon = imageSource as FileImageSource;

            if (icon == null)
            {
                return 0;
            }

            var file = System.IO.Path.GetFileNameWithoutExtension(icon.File);

            // Get the identifier (resource ID for the filename
            int drawableId = Resources.GetIdentifier(file, "drawable", Context.PackageName);

            return drawableId;
        }
    }
}
