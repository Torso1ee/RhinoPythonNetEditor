using Microsoft.Xaml.Behaviors.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace RhinoPythonNetEditor.View.Tools
{
    public class Dialog : ContentControl
    {
        private AdornerContainer container;
        private AdornerDecorator decorator;


        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
            "IsClosed", typeof(bool), typeof(Dialog), new PropertyMetadata(false));

        public bool IsClosed
        {
            get => (bool)GetValue(IsClosedProperty);
            internal set => SetValue(IsClosedProperty, value);
        }

        public static Dialog Show(Control control, object content)
        {
            var count = VisualTreeHelper.GetChildrenCount(control);
            AdornerDecorator decorator = null;
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                if (child is AdornerDecorator ad)
                {
                    decorator = ad;
                }
            }
            if (decorator != null)
            {
                if (decorator.Child != null)
                {
                    decorator.Child.IsEnabled = false;
                }
                var dialog = new Dialog
                {
                    Content = content
                };
                dialog.decorator = decorator;
                var layer = decorator.AdornerLayer;
                if (layer != null)
                {
                    var container = new AdornerContainer(layer)
                    {
                        Child = dialog
                    };
                    dialog.container = container;
                    layer.Add(container);
                }
                return dialog;
            }
            return null;
        }
     
        private void Close()
        {
            if (decorator != null && container != null)
            {
                if (decorator.Child != null)
                {
                    decorator.Child.IsEnabled = true;
                }
                var layer = decorator.AdornerLayer;
                IsClosed = true;
                Task.Delay(300).ContinueWith(t =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        layer?.Remove(container);
                    });
                });
            }
        }
    }

    public static class DialogExtension
    {
        public static Task WaitingForClosed(this Dialog dialog)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                if (dialog.IsClosed)
                {
                    SetResult();
                }
                else
                {
                    dialog.Unloaded += OnUnloaded;
                }
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }

            return tcs.Task;

            void OnUnloaded(object sender, RoutedEventArgs args)
            {
                dialog.Unloaded -= OnUnloaded;
                SetResult();
            }

            void SetResult()
            {
                try
                {
                    tcs.TrySetResult(dialog.IsClosed);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            }
        }
    }
}
