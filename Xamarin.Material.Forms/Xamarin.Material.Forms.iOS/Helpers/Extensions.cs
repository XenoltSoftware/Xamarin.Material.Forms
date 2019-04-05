using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Material.Forms.iOS
{
    public static class Extensions
    {
        internal static UIReturnKeyType ToUIReturnKeyType(this ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.Go:
                    return UIReturnKeyType.Go;
                case ReturnType.Next:
                    return UIReturnKeyType.Next;
                case ReturnType.Send:
                    return UIReturnKeyType.Send;
                case ReturnType.Search:
                    return UIReturnKeyType.Search;
                case ReturnType.Done:
                    return UIReturnKeyType.Done;
                case ReturnType.Default:
                    return UIReturnKeyType.Default;
                default:
                    throw new System.NotImplementedException($"ReturnType {returnType} not supported");
            }
        }

        public static T[] Remove<T>(this T[] self, T item)
        {
            return self.RemoveAt(self.IndexOf(item));
        }

        public static T[] Insert<T>(this T[] self, int index, T item)
        {
            var result = new T[self.Length + 1];
            if (index > 0)
                Array.Copy(self, result, index);

            result[index] = item;

            if (index < self.Length)
                Array.Copy(self, index, result, index + 1, result.Length - index - 1);

            return result;
        }

        public static T[] RemoveAt<T>(this T[] self, int index)
        {
            var result = new T[self.Length - 1];
            if (index > 0)
                Array.Copy(self, result, index);

            if (index < self.Length - 1)
                Array.Copy(self, index + 1, result, index, self.Length - index - 1);

            return result;
        }

        internal static async Task<UIImage> GetNativeImageAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
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

        internal static void DisposeModalAndChildRenderers(this Element view)
        {
            IVisualElementRenderer renderer;
            foreach (Element child in view.Descendants())
            {
                if (child is VisualElement ve)
                {
                    renderer = Platform.GetRenderer(ve);
                    //TODO
                    //child.ClearValue(Platform.RendererProperty);

                    if (renderer != null)
                    {
                        renderer.NativeView.RemoveFromSuperview();
                        renderer.Dispose();
                    }
                }
            }

            if (view is VisualElement visualElement)
            {
                renderer = Platform.GetRenderer(visualElement);
                if (renderer != null)
                {
                    if (renderer.ViewController != null)
                    {
                        var modalWrapper = renderer.ViewController.ParentViewController;
                        if (modalWrapper != null)
                            modalWrapper.Dispose();
                    }

                    renderer.NativeView.RemoveFromSuperview();
                    renderer.Dispose();
                }
                //TODO
                //view.ClearValue(Platform.RendererProperty);
            }
        }
    }
}
