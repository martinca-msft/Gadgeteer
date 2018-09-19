// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal static class Dwm
    {
        private static class Native
        {
            public static class Messages
            {
                /// <summary>
                /// Sent to all top-level windows when Desktop Window Manager (DWM) composition has been enabled or disabled.
                /// </summary>
                public const int DwmCompositionChanged = 0x031E;

                /// <summary>
                /// Informs all top-level windows that the colorization color has changed.
                /// </summary>
                public const int DwmColorizationColorChanged = 0x0320;

                /// <summary>
                /// Sent to a window in order to determine what part of the window corresponds to a particular screen coordinate. 
                /// </summary>
                public const int NonClientHitTest = 0x0084;
            }

            public static class HitTestResults
            {
                /// <summary>
                /// In a title bar.
                /// </summary>
                public static int Caption = 2;
            }

            private static readonly Version OsVersion = Environment.OSVersion.Version;
            private static readonly Version OsVersionColorizationParameters = new Version(6, 1);
            private static readonly Version MinimumOsVersion = new Version(6, 0);

            /// <summary>
            /// Specifies Desktop Window Manager (DWM) blur-behind properties. Used by the DwmEnableBlurBehindWindow function.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct BlurBehind
            {
                /// <summary>
                /// A bitwise combination that indicates which of the members of this structure have been set.
                /// </summary>
                public BlurBehindFlags Flags;
                /// <summary>
                /// true to register the window handle to DWM blur behind; false to unregister the window handle from DWM blur behind.
                /// </summary>
                public bool IsEnabled;
                /// <summary>
                /// The region within the client area where the blur behind will be applied. A NULL value will apply the blur behind the entire client area.
                /// </summary>
                public IntPtr Region;
                /// <summary>
                /// true if the window's colorization should transition to match the maximized windows; otherwise, false.
                /// </summary>
                public bool IsTransitionOnMaximized;
            }

            /// <summary>
            /// Flags used by the <see cref="BlurBehind" /> structure to indicate which of its members contain valid information.
            /// </summary>
            [Flags]
            private enum BlurBehindFlags
            {
                /// <summary>
                /// A value for the IsEnabled member has been specified.
                /// </summary>
                Enable = 0x00000001,
                /// <summary>
                /// A value for the Region member has been specified.
                /// </summary>
                Region = 0x00000002,
                /// <summary>
                /// A value for the IsTransitionOnMaximized member has been provider.
                /// </summary>
                TransitionOnMaximized = 0x00000004
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct ColorizationParameters
            {
                public int Color;
                public int Afterglow;
                public int ColorBalance;
                public int AfterglowBalance;
                public int BlurBalance;
                public int GlassReflectionIntensity;
                public int GlassAttribute;
            }

            /// <summary>
            /// Obtains a value that indicates whether Desktop Window Manager (DWM) composition is enabled.
            /// Applications can listen for composition state changes by handling the WM_DWMCOMPOSITIONCHANGED notification.
            /// </summary>
            /// <returns>true if DWM composition is enabled; false otherwise.</returns>
            [DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled", PreserveSig = false)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool _DwmIsCompositionEnabled();

            /// <summary>
            /// Enables the blur effect on the provided window handle.
            /// </summary>
            /// <param name="hwnd">The handle to the window for which the blur behind data is applied.</param>
            /// <param name="blurBehind">The blur behind data.</param>
            [DllImport("dwmapi.dll", EntryPoint = "DwmEnableBlurBehindWindow", PreserveSig = false)]
            private static extern void _DwmEnableBlurBehindWindow(IntPtr handle, ref BlurBehind blurBehind);

            /// <summary>
            /// Extends the window frame into the client area.
            /// </summary>
            /// <param name="handle">The handle to the window in which the frame will be extended into the client area.</param>
            /// <param name="margins">A pointer to a structure that describes the margins to use when extending the frame into the client area (left, top, right, bottom).</param>
            [DllImport("dwmapi.dll", EntryPoint = "DwmExtendFrameIntoClientArea", PreserveSig = false)]
            private static extern void _DwmExtendFrameIntoClientArea(IntPtr handle, params int[] margins);

            /// <summary>
            /// Retrieves the current color used for Desktop Window Manager (DWM) glass composition. This value is based on the current color scheme and can be modified by the user.
            /// </summary>
            /// <param name="color">A pointer to a value that, when this function returns successfully, receives the current color used for glass composition. The color format of the value is 0xAARRGGBB.</param>
            /// <param name="opaque">A pointer to a value that, when this function returns successfully, indicates whether the color is an opaque blend. true if the color is an opaque blend; otherwise, false.</param>
            [DllImport("dwmapi.dll", EntryPoint = "DwmGetColorizationColor", PreserveSig = false)]
            private static extern void _DwmGetColorizationColor(out int color, out int opaque);

            // Not available on Windows Vista and not needed starting Windows 8.
            [DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false)]
            private static extern void _DwmGetColorizationParameters(out ColorizationParameters parameters);

            public static bool IsDwmAvailable
            {
                get { return OsVersion >= MinimumOsVersion; }
            }
            public static bool IsDwmCompositionEnabled
            {
                get { return IsDwmAvailable && _DwmIsCompositionEnabled(); }
            }

            public static void EnableBlurBehindWindow(IntPtr handle, bool isEnabled, bool isMaximized, IntPtr region = default(IntPtr))
            {
                if (!IsDwmCompositionEnabled)
                    return;

                BlurBehind data = new BlurBehind
                {
                    IsEnabled = isEnabled,
                    IsTransitionOnMaximized = isMaximized,
                    Region = region
                };

                data.Flags = BlurBehindFlags.Enable | BlurBehindFlags.TransitionOnMaximized;
                if (region != IntPtr.Zero)
                    data.Flags |= BlurBehindFlags.Region;

                _DwmEnableBlurBehindWindow(handle, ref data);
            }
            public static void ExtendFrameIntoClientArea(IntPtr handle, Thickness thickness)
            {
                if (!IsDwmCompositionEnabled)
                    return;

                _DwmExtendFrameIntoClientArea(handle,
                    (int)Math.Ceiling(thickness.Left),
                    (int)Math.Ceiling(thickness.Right),
                    (int)Math.Ceiling(thickness.Top),
                    (int)Math.Ceiling(thickness.Bottom));
            }
            public static void ExtendFrameIntoClientArea(IntPtr handle, Rect bounds)
            {
                if (handle != IntPtr.Zero)
                {
                    int[] rect = new int[4];
                    if (_GetClientRect(handle, rect))
                        ExtendFrameIntoClientArea(handle, new Thickness(
                            Math.Max(0, bounds.Left),
                            Math.Max(0, bounds.Top),
                            Math.Max(0, rect[2] - bounds.Right),
                            Math.Max(0, rect[3] - bounds.Bottom)));
                }
            }

            public static bool GetColorizationColor(int wParam, int lParam, out Color color, out bool transparent)
            {
                color = ToColor(wParam);
                transparent = lParam != 0;

                if (!IsDwmCompositionEnabled)
                    return false;

                if (OsVersion == OsVersionColorizationParameters)
                {
                    ColorizationParameters p;
                    _DwmGetColorizationParameters(out p);

                    color = ToColor(p.Color);
                    transparent = p.GlassAttribute != 0;
                }

                return true;
            }
            public static bool GetColorizationColor(out Color color, out bool transparent)
            {
                color = Colors.Black;
                transparent = false;

                if (IsDwmCompositionEnabled)
                    return false;

                int colorValue;
                int opaqueValue;
                
                _DwmGetColorizationColor(out colorValue, out opaqueValue);

                color = ToColor(colorValue);
                transparent = opaqueValue != 0;

                return true;
            }
            public static bool GetColorizationColor(out int wParam, out int lParam)
            {
                wParam = 0;
                lParam = 0;

                if (IsDwmCompositionEnabled)
                {
                    _DwmGetColorizationColor(out wParam, out lParam);
                    return true;
                }

                return false;
            }

            private static Color ToColor(int value)
            {
                return Color.FromArgb((byte)((value & 0xFF000000) >> 24), (byte)((value & 0xFF0000) >> 16), (byte)((value & 0xFF00) >> 8), (byte)(value & 0xFF));
            }

            /// <summary>
            /// Retrieves the coordinates of a window's client area. The client coordinates specify the upper-left and lower-right corners of the client area. Because client coordinates are relative to the upper-left corner of a window's client area, the coordinates of the upper-left corner are (0,0).
            /// </summary>
            /// <param name="handle">A handle to the window whose client coordinates are to be retrieved. </param>
            /// <param name="rect">A pointer to a structure that receives the client coordinates (0, 0, width, height).</param>
            [DllImport("user32.dll", EntryPoint = "GetClientRect", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool _GetClientRect(IntPtr handle, int[] rect);

            public static Size GetClientSize(IntPtr handle)
            {
                int[] rect = new int[4];
                if (_GetClientRect(handle, rect))
                    return new Size(rect[2], rect[3]);
                else
                    return Size.Empty;
            }
        }

        private static ConcurrentDictionary<IntPtr, Window> TrackedWindows = new ConcurrentDictionary<IntPtr, Window>();

        private static bool EnsureWindowTracked(Window window)
        {
            WindowInteropHelper interop = new WindowInteropHelper(window);
            if (interop.Handle == IntPtr.Zero)
                window.SourceInitialized += EnsureWindowTracked;

            else if (TrackedWindows.TryAdd(interop.Handle, window))
            {
                window.SourceInitialized -= EnsureWindowTracked;
                window.Closed += EnsureWindowFreed;

                HwndSource source = HwndSource.FromHwnd(interop.Handle);
                if (source != null)
                    source.AddHook(DwmCompositionHook);

                OnIsCompositionEnabledChanged(window, Native.IsDwmCompositionEnabled);
            }

            else
                return true;

            return false;
        }
        private static void EnsureWindowTracked(object sender, EventArgs e)
        {
            EnsureWindowTracked((Window)sender);
        }

        private static void EnsureWindowFreed(Window window)
        {
            window.Closed -= EnsureWindowFreed;

            WindowInteropHelper interop = new WindowInteropHelper(window);
            if (interop.Handle == IntPtr.Zero)
                return;

            HwndSource source = HwndSource.FromHwnd(interop.Handle);
            if (source != null)
                source.RemoveHook(DwmCompositionHook);

            TrackedWindows.TryRemove(interop.Handle, out window);
        }
        private static void EnsureWindowFreed(object sender, EventArgs e)
        {
            EnsureWindowFreed((Window)sender);
        }

        private static IntPtr DwmCompositionHook(IntPtr handle, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Window window;
            switch (msg)
            {
                case Native.Messages.DwmCompositionChanged:
                    if (TrackedWindows.TryGetValue(handle, out window))
                        OnIsCompositionEnabledChanged(window, Native.IsDwmCompositionEnabled);
                    break;

                case Native.Messages.NonClientHitTest:
                    if (TrackedWindows.TryGetValue(handle, out window))
                    {
                        UIElement root = GetClientAreaRootElement(window);
                        if (root != null)
                        {
                            int point = lParam.ToInt32();
                            Point screenPoint = new Point(point & 0xFFFF, point >> 16);

                            Point rootPoint = root.PointFromScreen(screenPoint);
                            Size rootSize = root.RenderSize;
                            if (rootPoint.Y < 0 || rootPoint.X < 0 || rootPoint.X > rootSize.Width || rootPoint.Y > rootSize.Height)
                            {
                                Point windowPoint = window.PointFromScreen(screenPoint);
                                if (windowPoint.X >= 0 && windowPoint.Y >= 0)
                                {
                                    Size clientSize = Native.GetClientSize(handle);
                                    if (windowPoint.X < clientSize.Width && windowPoint.Y < clientSize.Height)
                                    {
                                        handled = true;
                                        return new IntPtr(Native.HitTestResults.Caption);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case Native.Messages.DwmColorizationColorChanged:
                    OnColorizationParametersInvalidated((int)wParam.ToInt64(), (int)lParam.ToInt64());
                    break;

            }
            return IntPtr.Zero;
        }

        #region Behind Blur

        private static DependencyPropertyDescriptor WindowStateChangedPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(Window.WindowStateProperty, typeof(Window));

        public static readonly DependencyProperty IsBlurBehindEnabledProperty = DependencyProperty.RegisterAttached("IsBlurBehindEnabled", typeof(bool), typeof(Dwm), new PropertyMetadata(false, OnIsBlurBehindEnabledPropertyChanged));
        public static bool GetIsBlurBehindEnabled(Window window)
        {
            return (bool)window.GetValue(IsBlurBehindEnabledProperty);
        }
        public static void SetIsBlurBehindEnabled(Window window, bool value)
        {
            window.SetValue(IsBlurBehindEnabledProperty, value);
        }

        private static void OnIsBlurBehindEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool value = (bool)e.NewValue;
            Window window = d as Window;
            if (window == null)
                return;

            if (value)
                WindowStateChangedPropertyDescriptor.AddValueChanged(window, OnBlurBehindWindowInvalidated);
            else
                WindowStateChangedPropertyDescriptor.RemoveValueChanged(window, OnBlurBehindWindowInvalidated);

            if (EnsureWindowTracked(window))
                OnBlurBehindWindowInvalidated(window);
        }

        private static void OnBlurBehindWindowInvalidated(Window window)
        {
            bool isEnabled = GetIsBlurBehindEnabled(window);

            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle != IntPtr.Zero)
                Native.EnableBlurBehindWindow(handle, isEnabled, window.WindowState == WindowState.Maximized);
        }
        private static void OnBlurBehindWindowInvalidated(object sender, EventArgs e)
        {
            OnBlurBehindWindowInvalidated((Window)sender);
        }

        #endregion

        #region Frame In Client Area

        public static readonly DependencyProperty ClientAreaRootElementProperty = DependencyProperty.RegisterAttached("ClientAreaRootElement", typeof(UIElement), typeof(Dwm), new PropertyMetadata(null, OnClientAreaRootElementPropertyChanged));
        public static UIElement GetClientAreaRootElement(Window window)
        {
            return (UIElement)window.GetValue(ClientAreaRootElementProperty);
        }
        public static void SetClientAreaRootElement(Window window, UIElement value)
        {
            window.SetValue(ClientAreaRootElementProperty, value);
        }

        private static void OnClientAreaRootElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = d as Window;
            if (window == null)
                return;

            UIElement oldElement = (UIElement)e.OldValue;
            if (oldElement != null)
                oldElement.LayoutUpdated -= OnFrameInClientAreaInvalidated;

            UIElement element = (UIElement)e.NewValue;
            if (element != null)
                element.LayoutUpdated += OnFrameInClientAreaInvalidated; // undesired multiple subscriptions if multiple windows

            if (EnsureWindowTracked(window))
                OnFrameInClientAreaInvalidated(window);
        }

        private static void OnFrameInClientAreaInvalidated(Window window)
        {
            UIElement rootElement = GetClientAreaRootElement(window);

            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle != IntPtr.Zero)
                if (rootElement == null)
                    Native.ExtendFrameIntoClientArea(handle, new Thickness());
                else if (window.IsLoaded)
                {                    
                    Rect rootBounds = rootElement.TransformToAncestor(window).TransformBounds(new Rect(rootElement.RenderSize));

                    Native.ExtendFrameIntoClientArea(handle, rootBounds);
                }
        }
        private static void OnFrameInClientAreaInvalidated(object sender, EventArgs e)
        {
            Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;

            Window[] trackedWindows = new Window[TrackedWindows.Count];
            TrackedWindows.Values.CopyTo(trackedWindows, 0);

            foreach (Window trackedWindow in trackedWindows)
                if (trackedWindow.Dispatcher == currentDispatcher)
                    OnFrameInClientAreaInvalidated(trackedWindow);
        }

        #endregion

        #region Colorization

        public static string ColorizationColorKey { get { return "ColorizationColor"; } }
        public static string ColorizationBrushKey { get { return "ColorizationBrush"; } }
        public static string ColorizationOpaqueColorKey { get { return "ColorizationOpaqueColor"; } }
        public static string ColorizationOpaqueBrushKey { get { return "ColorizationOpaqueBrush"; } }

        private static void OnColorizationParametersInvalidated(int wParam, int lParam)
        {
            Color color;
            bool transparent;

            if (Native.GetColorizationColor(wParam, lParam, out color, out transparent))
            {
                Color opaqueColor = Color.FromRgb(color.R, color.G, color.B);

                SolidColorBrush brush = new SolidColorBrush(color);
                brush.Freeze();

                SolidColorBrush opaqueBrush = new SolidColorBrush(opaqueColor);
                opaqueBrush.Freeze();

                foreach (Window window in TrackedWindows.Values)
                {
                    window.Resources[ColorizationColorKey] = color;
                    window.Resources[ColorizationBrushKey] = brush;
                    window.Resources[ColorizationOpaqueColorKey] = opaqueColor;
                    window.Resources[ColorizationOpaqueBrushKey] = opaqueBrush;
                }
            }
            else
                foreach (Window window in TrackedWindows.Values)
                {
                    window.Resources.Remove(ColorizationColorKey);
                    window.Resources.Remove(ColorizationBrushKey);
                    window.Resources.Remove(ColorizationOpaqueColorKey);
                    window.Resources.Remove(ColorizationOpaqueBrushKey);
                }
        }
        private static void OnColorizationParametersInvalidated()
        {
            int wParam, lParam;
         
            if (Native.GetColorizationColor(out wParam, out lParam))
                OnColorizationParametersInvalidated(wParam, lParam);
        }

        #endregion

        private static void OnIsCompositionEnabledChanged(Window window, bool isEnabled)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;

            if (handle != IntPtr.Zero)
                HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = isEnabled ? Colors.Transparent : SystemColors.WindowColor;

            OnBlurBehindWindowInvalidated(window);
            OnFrameInClientAreaInvalidated(window);
            OnColorizationParametersInvalidated();
        }
    }
}
