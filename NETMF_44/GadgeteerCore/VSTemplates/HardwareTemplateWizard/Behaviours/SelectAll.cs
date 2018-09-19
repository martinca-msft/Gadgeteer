// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class SelectAll : DependencyObject
    {
        public static readonly DependencyProperty OnGotFocusProperty = DependencyProperty.RegisterAttached("OnGotFocus", typeof(bool), typeof(SelectAll), new PropertyMetadata(OnGetFocusPropertyChanged));

        public static bool GetOnGotFocus(TextBoxBase obj)
        {
            return (bool)obj.GetValue(OnGotFocusProperty);
        }

        public static void SetOnGotFocus(TextBoxBase obj, bool value)
        {
            obj.SetValue(OnGotFocusProperty, value);
        }

        private static void OnGetFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxBase box = d as TextBoxBase;

            if (box != null)
            {
                if ((bool)e.NewValue)
                    box.GotFocus += OnSelectAll;
                else
                    box.GotFocus -= OnSelectAll;
            }
        }

        private static void OnSelectAll(object sender, RoutedEventArgs e)
        {
            TextBoxBase box = sender as TextBoxBase;

            if (box != null)
                box.SelectAll();
        }



    }
}
