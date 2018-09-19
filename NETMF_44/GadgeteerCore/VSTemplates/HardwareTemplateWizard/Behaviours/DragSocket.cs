// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class DragSocket : DependencyObject
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached("Target", typeof(SocketBaseViewModel), typeof(DragSocket), new PropertyMetadata(OnTargetPropertyChanged));
        private static readonly DependencyProperty TargetStartProperty = DependencyProperty.RegisterAttached("TargetStart", typeof(Point), typeof(DragSocket));
        private static readonly DependencyProperty DragStartProperty = DependencyProperty.RegisterAttached("DragStart", typeof(Point), typeof(DragSocket));

        public static SocketBaseViewModel GetTarget(DependencyObject obj)
        {
            return (SocketBaseViewModel)obj.GetValue(TargetProperty);
        }
        public static void SetTarget(DependencyObject obj, SocketBaseViewModel value)
        {
            obj.SetValue(TargetProperty, value);
        }
        private static Point GetTargetStart(DependencyObject obj)
        {
            return (Point)obj.GetValue(TargetStartProperty);
        }
        private static void SetTargetStart(DependencyObject obj, Point value)
        {
            obj.SetValue(TargetStartProperty, value);
        }
        private static Point GetDragStart(DependencyObject obj)
        {
            return (Point)obj.GetValue(DragStartProperty);
        }
        private static void SetDragStart(DependencyObject obj, Point value)
        {
            obj.SetValue(DragStartProperty, value);
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement el = d as FrameworkElement;

            if (el != null)
            {
                if (e.NewValue != null)
                {
                    el.Cursor = Cursors.Hand;
                    el.PreviewMouseLeftButtonDown += OnDragStart;
                    el.PreviewMouseMove += OnDragMove;
                    el.PreviewMouseLeftButtonUp += OnDragEnd;
                    el.PreviewKeyDown += OnDragKey;
                }
                else
                {
                    el.ClearValue(FrameworkElement.CursorProperty);
                    el.PreviewKeyDown -= OnDragKey;
                    el.PreviewMouseLeftButtonUp -= OnDragEnd;
                    el.PreviewMouseMove -= OnDragMove;
                    el.PreviewMouseLeftButtonDown -= OnDragStart;
                }
            }
        }

        private static IInputElement GetAnchor(FrameworkElement el)
        {
            return (el.Parent as FrameworkElement).Parent as IInputElement;
        }

        private static void OnDragStart(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement el = (FrameworkElement)sender;

            if (el.Focusable)
                el.Focus();

            if (el.CaptureMouse())
            {
                SetDragStart(el, e.GetPosition(GetAnchor(el)));

                SocketBaseViewModel socket = GetTarget(el);
                if (socket != null)
                    SetTargetStart(el, new Point(socket.Left, socket.Top));

                e.Handled = true;
            }
        }
        private static void OnDragMove(object sender, MouseEventArgs e)
        {
            FrameworkElement el = (FrameworkElement)sender;
            Point origin = GetTargetStart(el);

            if (origin != default(Point))
            {
                SocketBaseViewModel socket = GetTarget(el);
                if (socket != null)
                {
                    Vector distance = e.GetPosition(GetAnchor(el)) - GetDragStart(el);

                    socket.Left = OneDecimal(origin.X + distance.X * 25.4 / 96.0);
                    socket.Top = OneDecimal(origin.Y + distance.Y * 25.4 / 96.0);

                    e.Handled = true;
                }
            }
        }

        private static void OnDragEnd(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement el = (FrameworkElement)sender;
            if (el.IsMouseCaptured)
            {
                el.ReleaseMouseCapture();

                el.ClearValue(DragStartProperty);
                el.ClearValue(TargetStartProperty);

                e.Handled = true;
            }
        }

        private static void OnDragKey(object sender, KeyEventArgs e)
        {
            SocketBaseViewModel socket = GetTarget(sender as FrameworkElement);

            double delta = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) ? 5.0 : 0.1;

            switch (e.Key)
            {
                case Key.Left:
                    socket.Left = OneDecimal(socket.Left - delta);
                    e.Handled = true;
                    break;

                case Key.Right:
                    socket.Left = OneDecimal(socket.Left + delta);
                    e.Handled = true;
                    break;

                case Key.Up:
                    socket.Top = OneDecimal(socket.Top - delta);
                    e.Handled = true;
                    break;

                case Key.Down:
                    socket.Top = OneDecimal(socket.Top + delta);
                    e.Handled = true;
                    break;

                case Key.Space:
                    socket.Notch = (Dock)(((int)socket.Notch + 1) % 4);
                    break;

                default:
                    break;
            }
        }

        private static double OneDecimal(double d)
        {
            return Math.Round(d * 10) / 10;
        }
    }
}
