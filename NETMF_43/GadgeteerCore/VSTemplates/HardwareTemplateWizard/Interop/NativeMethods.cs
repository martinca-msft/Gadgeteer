// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    internal class NativeMethods
    {
        #region Desktop Window Manager

        [DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", EntryPoint = "DwmExtendFrameIntoClientArea", PreserveSig = false)]
        private static extern void _DwmExtendFrameIntoClientArea(IntPtr hwnd, int[] pMarInset);

        private static readonly Version _osVersion = Environment.OSVersion.Version;
        private static readonly Version _vistaVersion = new Version(6, 0);

        public static bool IsDwmAvailable
        {
            get { return _osVersion >= _vistaVersion; }
        }

        public static bool IsDwmIsCompositionEnabled
        {
            get { return IsDwmAvailable && _DwmIsCompositionEnabled(); }
        }

        public static void DwmExtendFrameIntoClientArea(IntPtr hwnd, int left, int top, int right, int bottom)
        {
            int[] margins = new int[] { left, top, right, bottom };
            _DwmExtendFrameIntoClientArea(hwnd, margins);
        }

        #endregion

        #region Shell

        private const uint StockHandle = 0x00000100;
        private const uint StockSmallIcon = 0x00000001;

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, SetLastError = false, PreserveSig = true)]
        private static extern int SHGetStockIconInfo(int identifier, uint flags, ref StockIconInfo info);

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StockIconInfo
        {
            internal int Size;
            internal IntPtr Handle;
            internal int SystemIconIndex;
            internal int IconIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string Path;
        }

        private class SafeIconHandle : SafeHandle
        {
            public SafeIconHandle() : base(IntPtr.Zero, true) { }
            public SafeIconHandle(IntPtr handle) : base(handle, true) { }

            public override bool IsInvalid
            {
                get { return handle == IntPtr.Zero; }
            }

            protected override bool ReleaseHandle()
            {
                return DestroyIcon(handle);
            }
        }

        internal static SafeHandle GetStockSmallIconHandle(int identifier)
        {
            StockIconInfo info = new StockIconInfo();
            info.Size = Marshal.SizeOf(typeof(StockIconInfo));

            if (SHGetStockIconInfo(identifier, StockSmallIcon | StockHandle, ref info) == 0)
                return new SafeIconHandle(info.Handle);

            return new SafeIconHandle();
        }

        #endregion
    }
}
