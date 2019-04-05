using System;
using Xamarin.Forms;
using Xamarin.Material.Forms;
using Xamarin.Forms.Platform.iOS;
using MTextField = MaterialComponents.TextField;
using MTextInputControllerOutlined = MaterialComponents.TextInputControllerOutlined;
using MTextInputControllerFilled = MaterialComponents.TextInputControllerFilled;
using MTextInputControllerBase = MaterialComponents.TextInputControllerBase;
using MTextInputControllerUnderline = MaterialComponents.TextInputControllerUnderline;
using UIKit;
using System.Drawing;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Specifics = Xamarin.Forms.PlatformConfiguration.iOSSpecific.Entry;
using Xamarin.Material.Forms.iOS.Helpers;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms.Internals;

[assembly: ExportRenderer(typeof(TextField), typeof(Xamarin.Material.Forms.iOS.Renderers.TextFieldRenderer))]
namespace Xamarin.Material.Forms.iOS.Renderers
{
    public class TextFieldRenderer : ViewRenderer<TextField, MTextField>
    {
        private MTextInputControllerBase textInputController;


        UIColor _defaultTextColor;

        // Placeholder default color is 70% gray
        // https://developer.apple.com/library/prerelease/ios/documentation/UIKit/Reference/UITextField_Class/index.html#//apple_ref/occ/instp/UITextField/placeholder
        readonly Color _defaultPlaceholderColor = ColorExtensions.SeventyPercentGrey.ToColor();
        UIColor _defaultCursorColor;
        bool _useLegacyColorManagement;

        bool _disposed;
        IDisposable _selectedTextRangeObserver;
        bool _nativeSelectionIsUpdating;

        bool _cursorPositionChangePending;
        bool _selectionLengthChangePending;

        static readonly int baseHeight = 30;
        static CGSize initialSize = CGSize.Empty;

        public TextFieldRenderer()
        {
            Frame = new RectangleF(0, 20, 320, 40);
        }

        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var baseResult = base.GetDesiredSize(widthConstraint, heightConstraint);

            if (FormsHelper.IsiOS11OrNewer)
                return baseResult;

            NSString testString = new NSString("Tj");
            var testSize = testString.GetSizeUsingAttributes(new UIStringAttributes { Font = Control.Font });
            double height = baseHeight + testSize.Height - initialSize.Height;
            height = Math.Round(height);

            return new SizeRequest(new Xamarin.Forms.Size(baseResult.Request.Width, height));
        }

        IElementController ElementController => Element as IElementController;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (disposing)
            {
                _defaultTextColor = null;

                if (Control != null)
                {
                    _defaultCursorColor = Control.TintColor;
                    Control.EditingDidBegin -= OnEditingBegan;
                    Control.EditingChanged -= OnEditingChanged;
                    Control.EditingDidEnd -= OnEditingEnded;
                    Control.ShouldChangeCharacters -= ShouldChangeCharacters;
                    _selectedTextRangeObserver?.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TextField> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
                return;

            if (Control == null)
            {
                var textField = CreateNativeControl();
                SetNativeControl(textField);

                // Cache the default text color
                _defaultTextColor = textField.TextColor;

                //ToDo
                //_useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
                _useLegacyColorManagement = true;

                textField.EditingChanged += OnEditingChanged;
                textField.ShouldReturn = OnShouldReturn;

                textField.EditingDidBegin += OnEditingBegan;
                textField.EditingDidEnd += OnEditingEnded;
                textField.ShouldChangeCharacters += ShouldChangeCharacters;
                _selectedTextRangeObserver = textField.AddObserver("selectedTextRange", NSKeyValueObservingOptions.New, UpdateCursorFromControl);

            }

            // When we set the control text, it triggers the UpdateCursorFromControl event, which updates CursorPosition and SelectionLength;
            // These one-time-use variables will let us initialize a CursorPosition and SelectionLength via ctor/xaml/etc.
            _cursorPositionChangePending = Element.IsSet(TextField.CursorPositionProperty);
            _selectionLengthChangePending = Element.IsSet(TextField.SelectionLengthProperty);

            UpdatePlaceholder();
            UpdatePassword();
            UpdateText();
            UpdateColor();
            UpdateFont();
            UpdateKeyboard();
            UpdateAlignment();
            UpdateAdjustsFontSizeToFitWidth();
            UpdateMaxLength();
            UpdateReturnType();
            UpdateBorderType();
            UpdateFloatingEnabled();
            UpdateHelperText();
            UpdateErrorText();
            UpdateCharacterCountMax();
            UpdateLeadingIcon();
            UpdateTrailingIcon();

            if (_cursorPositionChangePending || _selectionLengthChangePending)
                UpdateCursorSelection();

            UpdateCursorColor();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TextField.PlaceholderProperty.PropertyName || e.PropertyName == TextField.PlaceholderColorProperty.PropertyName)
                UpdatePlaceholder();
            else if (e.PropertyName == TextField.IsPasswordProperty.PropertyName)
                UpdatePassword();
            else if (e.PropertyName == TextField.TextProperty.PropertyName)
                UpdateText();
            else if (e.PropertyName == TextField.TextColorProperty.PropertyName)
                UpdateColor();
            else if (e.PropertyName == Xamarin.Forms.InputView.KeyboardProperty.PropertyName)
                UpdateKeyboard();
            else if (e.PropertyName == Xamarin.Forms.InputView.IsSpellCheckEnabledProperty.PropertyName)
                UpdateKeyboard();
            else if (e.PropertyName == TextField.IsTextPredictionEnabledProperty.PropertyName)
                UpdateKeyboard();
            else if (e.PropertyName == TextField.HorizontalTextAlignmentProperty.PropertyName)
                UpdateAlignment();
            else if (e.PropertyName == TextField.FontAttributesProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == TextField.FontFamilyProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == TextField.FontSizeProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == Xamarin.Forms.VisualElement.IsEnabledProperty.PropertyName)
            {
                UpdateColor();
                UpdatePlaceholder();
            }
            else if (e.PropertyName == Specifics.AdjustsFontSizeToFitWidthProperty.PropertyName)
                UpdateAdjustsFontSizeToFitWidth();
            else if (e.PropertyName == Xamarin.Forms.VisualElement.FlowDirectionProperty.PropertyName)
                UpdateAlignment();
            else if (e.PropertyName == Xamarin.Forms.InputView.MaxLengthProperty.PropertyName)
                UpdateMaxLength();
            else if (e.PropertyName == TextField.ReturnTypeProperty.PropertyName)
                UpdateReturnType();
            else if (e.PropertyName == TextField.CursorPositionProperty.PropertyName)
                UpdateCursorSelection();
            else if (e.PropertyName == TextField.SelectionLengthProperty.PropertyName)
                UpdateCursorSelection();
            else if (e.PropertyName == Specifics.CursorColorProperty.PropertyName)
                UpdateCursorColor();
            else if (e.PropertyName == nameof(Element.BorderType))
                UpdateBorderType();
            else if (e.PropertyName == nameof(Element.FloatingEnabled))
                UpdateFloatingEnabled();
            else if (e.PropertyName == nameof(Element.HelperText))
                UpdateHelperText();
            else if (e.PropertyName == nameof(Element.Error))
                UpdateErrorText();
            else if (e.PropertyName == nameof(Element.CharacterCountMax))
                UpdateCharacterCountMax();
            else if (e.PropertyName == nameof(Element.LeadingIcon))
                UpdateLeadingIcon();
            else if (e.PropertyName == nameof(Element.TrailingIcon))
                UpdateTrailingIcon();

            base.OnElementPropertyChanged(sender, e);
        }

        void OnEditingBegan(object sender, EventArgs e)
        {
            if (!_cursorPositionChangePending && !_selectionLengthChangePending)
                UpdateCursorFromControl(null);
            else
                UpdateCursorSelection();

            ElementController.SetValueFromRenderer(Xamarin.Forms.VisualElement.IsFocusedPropertyKey, true);
        }

        void OnEditingChanged(object sender, EventArgs eventArgs)
        {
            ElementController.SetValueFromRenderer(TextField.TextProperty, Control.Text);
            UpdateCursorFromControl(null);
        }

        void OnEditingEnded(object sender, EventArgs e)
        {
            // Typing aid changes don't always raise EditingChanged event

            // Normalizing nulls to string.Empty allows us to ensure that a change from null to "" doesn't result in a change event.
            // While technically this is a difference it serves no functional good.
            var controlText = Control.Text ?? string.Empty;
            var entryText = Element.Text ?? string.Empty;
            if (controlText != entryText)
            {
                ElementController.SetValueFromRenderer(TextField.TextProperty, controlText);
            }

            ElementController.SetValueFromRenderer(Xamarin.Forms.VisualElement.IsFocusedPropertyKey, false);
        }

        protected virtual bool OnShouldReturn(UITextField view)
        {
            Control.ResignFirstResponder();
            ((IEntryController)Element).SendCompleted();
            return false;
        }

        void UpdateAlignment()
        {
            Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
        }

        void UpdateColor()
        {
            var textColor = Element.TextColor;

            if (_useLegacyColorManagement)
            {
                Control.TextColor = textColor.IsDefault || !Element.IsEnabled ? _defaultTextColor : textColor.ToUIColor();
            }
            else
            {
                Control.TextColor = textColor.IsDefault ? _defaultTextColor : textColor.ToUIColor();
            }
        }

        void UpdateAdjustsFontSizeToFitWidth()
        {
            //ToDo
            //Control.AdjustsFontSizeToFitWidth = Element.OnThisPlatform().AdjustsFontSizeToFitWidth();
        }

        void UpdateFont()
        {
            if (initialSize == CGSize.Empty)
            {
                NSString testString = new NSString("Tj");
                initialSize = testString.StringSize(Control.Font);
            }

            Control.Font = Element.ToUIFont();
        }

        void UpdateKeyboard()
        {
            var keyboard = Element.Keyboard;
            Control.ApplyKeyboard(keyboard);
            if (!(keyboard is Xamarin.Forms.Internals.CustomKeyboard))
            {
                if (Element.IsSet(Xamarin.Forms.InputView.IsSpellCheckEnabledProperty))
                {
                    if (!Element.IsSpellCheckEnabled)
                    {
                        Control.SpellCheckingType = UITextSpellCheckingType.No;
                    }
                }
                if (Element.IsSet(TextField.IsTextPredictionEnabledProperty))
                {
                    if (!Element.IsTextPredictionEnabled)
                    {
                        Control.AutocorrectionType = UITextAutocorrectionType.No;
                    }
                }
            }
            Control.ReloadInputViews();
        }

        void UpdatePassword()
        {
            if (Element.IsPassword && Control.IsFirstResponder)
            {
                Control.Enabled = false;
                Control.SecureTextEntry = true;
                Control.Enabled = Element.IsEnabled;
                Control.BecomeFirstResponder();
            }
            else
                Control.SecureTextEntry = Element.IsPassword;
        }

        void UpdatePlaceholder()
        {
            var formatted = (FormattedString)Element.Placeholder;

            if (formatted == null)
                return;

            var targetColor = Element.PlaceholderColor;

            if (_useLegacyColorManagement)
            {
                var color = targetColor.IsDefault || !Element.IsEnabled ? _defaultPlaceholderColor : targetColor;
                Control.AttributedPlaceholder = formatted.ToAttributed(Element, color);
            }
            else
            {
                // Using VSM color management; take whatever is in Element.PlaceholderColor
                var color = targetColor.IsDefault ? _defaultPlaceholderColor : targetColor;
                Control.AttributedPlaceholder = formatted.ToAttributed(Element, color);
            }
        }

        void UpdateText()
        {
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (Control.Text != Element.Text)
                Control.Text = Element.Text;
        }

        void UpdateMaxLength()
        {
            var currentControlText = Control.Text;

            if (currentControlText.Length > Element.MaxLength)
                Control.Text = currentControlText.Substring(0, Element.MaxLength);
        }

        bool ShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
        {
            var newLength = textField?.Text?.Length + replacementString.Length - range.Length;
            return newLength <= Element?.MaxLength;
        }

        void UpdateReturnType()
        {
            if (Control == null || Element == null)
                return;
            Control.ReturnKeyType = Element.ReturnType.ToUIReturnKeyType();
        }

        void UpdateCursorFromControl(NSObservedChange obj)
        {
            if (_nativeSelectionIsUpdating || Control == null || Element == null)
                return;

            var currentSelection = Control.SelectedTextRange;
            if (currentSelection != null)
            {
                if (!_cursorPositionChangePending)
                {
                    int newCursorPosition = (int)Control.GetOffsetFromPosition(Control.BeginningOfDocument, currentSelection.Start);
                    if (newCursorPosition != Element.CursorPosition)
                        SetCursorPositionFromRenderer(newCursorPosition);
                }

                if (!_selectionLengthChangePending)
                {
                    int selectionLength = (int)Control.GetOffsetFromPosition(currentSelection.Start, currentSelection.End);

                    if (selectionLength != Element.SelectionLength)
                        SetSelectionLengthFromRenderer(selectionLength);
                }
            }
        }

        void UpdateCursorSelection()
        {
            if (_nativeSelectionIsUpdating || Control == null || Element == null)
                return;

            _cursorPositionChangePending = _selectionLengthChangePending = true;

            // If this is run from the ctor, the control is likely too early in its lifecycle to be first responder yet. 
            // Anything done here will have no effect, so we'll skip this work until later.
            // We'll try again when the control does become first responder later OnEditingBegan
            if (Control.BecomeFirstResponder())
            {
                try
                {
                    int cursorPosition = Element.CursorPosition;

                    UITextPosition start = GetSelectionStart(cursorPosition, out int startOffset);
                    UITextPosition end = GetSelectionEnd(cursorPosition, start, startOffset);

                    Control.SelectedTextRange = Control.GetTextRange(start, end);
                }
                catch (Exception ex)
                {
                    Xamarin.Forms.Internals.Log.Warning("Entry", $"Failed to set Control.SelectedTextRange from CursorPosition/SelectionLength: {ex}");
                }
                finally
                {
                    _cursorPositionChangePending = _selectionLengthChangePending = false;
                }
            }
        }

        UITextPosition GetSelectionEnd(int cursorPosition, UITextPosition start, int startOffset)
        {
            UITextPosition end = start;
            int endOffset = startOffset;
            int selectionLength = Element.SelectionLength;

            if (Element.IsSet(TextField.SelectionLengthProperty))
            {
                end = Control.GetPosition(start, Math.Max(startOffset, Math.Min(Control.Text.Length - cursorPosition, selectionLength))) ?? start;
                endOffset = Math.Max(startOffset, (int)Control.GetOffsetFromPosition(Control.BeginningOfDocument, end));
            }

            int newSelectionLength = Math.Max(0, endOffset - startOffset);
            if (newSelectionLength != selectionLength)
                SetSelectionLengthFromRenderer(newSelectionLength);

            return end;
        }

        UITextPosition GetSelectionStart(int cursorPosition, out int startOffset)
        {
            UITextPosition start = Control.EndOfDocument;
            startOffset = Control.Text.Length;

            if (Element.IsSet(TextField.CursorPositionProperty))
            {
                start = Control.GetPosition(Control.BeginningOfDocument, cursorPosition) ?? Control.EndOfDocument;
                startOffset = Math.Max(0, (int)Control.GetOffsetFromPosition(Control.BeginningOfDocument, start));
            }

            if (startOffset != cursorPosition)
                SetCursorPositionFromRenderer(startOffset);

            return start;
        }

        void UpdateCursorColor()
        {
            var control = Control;
            if (control == null || Element == null)
                return;

            if (Element.IsSet(Specifics.CursorColorProperty))
            {
                //ToDo
                //var color = Element.OnThisPlatform().GetCursorColor();
                //if (color == Color.Default)
                //    control.TintColor = _defaultCursorColor;
                //else
                    //control.TintColor = color.ToUIColor();
            }
        }

        void SetCursorPositionFromRenderer(int start)
        {
            try
            {
                _nativeSelectionIsUpdating = true;
                ElementController?.SetValueFromRenderer(TextField.CursorPositionProperty, start);
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                Xamarin.Forms.Internals.Log.Warning("Entry", $"Failed to set SelectionLength from renderer: {ex}");
            }
            finally
            {
                _nativeSelectionIsUpdating = false;
            }
        }



        protected override MTextField CreateNativeControl()
        {
            var textField = new MTextField();

            textField.BorderStyle = UIKit.UITextBorderStyle.None;
            textField.ClearButtonMode = UIKit.UITextFieldViewMode.Never;
            return textField;
        }

        private void UpdateBorderType()
        {
            if (Element.BorderType == TextFieldBorderType.Filled)
            {
                textInputController = new MTextInputControllerFilled(Control);
            }
            else if (Element.BorderType == TextFieldBorderType.Outline)
            {
                textInputController = new MTextInputControllerOutlined(Control);
            }
            else
            {
                textInputController = new MTextInputControllerUnderline(Control);
            }
        }

        private void UpdateFloatingEnabled()
        {
            textInputController.FloatingEnabled = Element.FloatingEnabled;
        }

        private void UpdateHelperText()
        {
            textInputController.HelperText = Element.HelperText;
        }

        private void UpdateErrorText()
        {
            textInputController.SetErrorText(Element.Error, null);
        }

        private void UpdateCharacterCountMax()
        {
            textInputController.CharacterCountMax = Element.CharacterCountMax == null ?
            (nuint)0 : (nuint)Element.CharacterCountMax.Value;
        }

        private async void UpdateLeadingIcon()
        {
            if (Element?.LeadingIcon == null)
                return;
            try
            {
                var uiImage = await GetNativeImageAsync(Element.LeadingIcon).ConfigureAwait(false);
                var imageView = new UIKit.UIImageView(new CoreGraphics.CGRect(0, 0, 18, 18));
                imageView.Image = uiImage;
                Control.LeftView = imageView;
                Control.LeftViewMode = UIKit.UITextFieldViewMode.Always;
            }
            catch (Exception ex)
            {
                Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
            }
        }

        private async void UpdateTrailingIcon()
        {
            if (Element?.TrailingIcon == null)
                return;
            try
            {
                var uiImage = await GetNativeImageAsync(Element.TrailingIcon).ConfigureAwait(false);
                var imageView = new UIKit.UIImageView(new CoreGraphics.CGRect(0, 0, 18, 18));
                imageView.Image = uiImage;
                Control.RightView = imageView;
                Control.RightViewMode = UIKit.UITextFieldViewMode.Always;
            }
            catch (Exception ex)
            {
                Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
            }
        }

        internal static async Task<UIImage> GetNativeImageAsync(ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
        {
            IImageSourceHandler handler;
            if (source != null && (handler = Xamarin.Forms.Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
            {
                try
                {
                    return await handler.LoadImageAsync(source, scale: (float)UIScreen.MainScreen.Scale, cancelationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // no-op
                }
            }

            return null;
        }
    }


}
