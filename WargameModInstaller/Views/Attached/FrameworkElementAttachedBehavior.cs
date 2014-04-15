using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WargameModInstaller.Views.Attached
{
    class FrameworkElementAttachedBehavior
    {
        public static readonly DependencyProperty FocusWhenLoadedProperty = DependencyProperty.RegisterAttached(
            "FocusWhenLoaded",
            typeof(bool),
            typeof(FrameworkElementAttachedBehavior),
            new UIPropertyMetadata(false, OnFocusWhenLoadedChanged));

        public static bool GetFocusWhenLoaded(FrameworkElement element)
        {
            return (bool)element.GetValue(FocusWhenLoadedProperty);
        }

        public static void SetFocusWhenLoaded(FrameworkElement element, bool value)
        {
            element.SetValue(FocusWhenLoadedProperty, value);
        }

        private static void OnFocusWhenLoadedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                if ((bool)e.NewValue == true)
                {
                    element.Loaded += ElementLoadedHandler;
                }
                else
                {
                    element.Loaded -= ElementLoadedHandler;
                }
            }
        }

        private static void ElementLoadedHandler(object sender, EventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                element.Focus();
            }
        }

    }
}
