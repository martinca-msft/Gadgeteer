// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class SocketBaseViewModel : DependencyObject
    {
        public static readonly DependencyProperty NotchProperty = DependencyProperty.Register("Notch", typeof(Dock), typeof(SocketBaseViewModel), new PropertyMetadata(Dock.Right, OnNotchPropertyChanged));
        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof(double), typeof(SocketBaseViewModel), new PropertyMetadata(10.0, MarkDirty), ValidateFinite);
        public static readonly DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof(double), typeof(SocketBaseViewModel), new PropertyMetadata(10.0, MarkDirty), ValidateFinite);

        private static bool ValidateFinite(object value)
        {
            double d = (double)value;
            return !double.IsInfinity(d) && !double.IsNaN(d);
        }

        protected static void MarkDirty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SocketBaseViewModel)d).IsDirty = true;
        }

        private static readonly DependencyPropertyKey OrientationPropertyKey = DependencyProperty.RegisterReadOnly("Orientation", typeof(double), typeof(SocketBaseViewModel), new PropertyMetadata(0.0, MarkDirty)); // keep in sync with Notch default
        public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

        private static void OnNotchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SocketBaseViewModel socket = (SocketBaseViewModel)d;

            switch ((Dock)e.NewValue)
            {
                case Dock.Right:
                    socket.Orientation = 0;
                    break;

                case Dock.Top:
                    socket.Orientation = 90;
                    break;

                case Dock.Left:                    
                    socket.Orientation = 180;
                    break;

                case Dock.Bottom:
                    socket.Orientation = 270;
                    break;
            }
        }

        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        public Dock Notch
        {
            get { return (Dock)GetValue(NotchProperty); }
            set { SetValue(NotchProperty, value); }
        }

        public double Orientation
        {
            get { return (double)GetValue(OrientationProperty); }
            private set { SetValue(OrientationPropertyKey, value); }
        }

        internal bool IsDirty;
    }
}
