// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class Sync : DependencyObject
    {
        public static readonly DependencyProperty WhenEqualToProperty = DependencyProperty.RegisterAttached("WhenEqualTo", typeof(string), typeof(Sync), new PropertyMetadata(OnWhenEqualToPropertyChanged));

        public static string GetWhenEqualTo(TextBox destination)
        {
            return (string)destination.GetValue(WhenEqualToProperty);
        }
        public static void SetWhenEqualTo(TextBox destination, string source)
        {
            destination.SetValue(WhenEqualToProperty, source);
        }

        private static void OnWhenEqualToPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox destination = d as TextBox;
            if (destination != null)
            {
                string oldValue = (string)e.OldValue ?? string.Empty;
                string newValue = (string)e.NewValue ?? string.Empty;

                if (destination.Text == oldValue)
                    destination.Text = newValue;
            }
        }
    }
}
