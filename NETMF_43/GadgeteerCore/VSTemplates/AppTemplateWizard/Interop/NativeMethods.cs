// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Microsoft.Win32;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal class NativeMethods
    {
        private static readonly Version _osVersion = Environment.OSVersion.Version;
        private static readonly Version _vistaVersion = new Version(6, 0);

        private const uint StockHandle = 0x00000100;
        private const uint StockSmallIcon = 0x00000001;

        [DllImport("User32.dll", EntryPoint="DestroyIcon", SetLastError = true)]
        private static extern bool _DestroyIcon(IntPtr handle);

        [DllImport("Shell32.dll", EntryPoint="SHGetStockIconInfo", CharSet = CharSet.Unicode, SetLastError = false, PreserveSig = true)]
        private static extern int _GetStockIconInfo(int identifier, uint flags, ref StockIconInfo info);

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

        public static bool IsStockIconAvailable
        {
            get { return _osVersion >= _vistaVersion; }
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
                return _DestroyIcon(handle);
            }
        }

        internal static SafeHandle GetStockSmallIconHandle(int identifier)
        {
            if (!IsStockIconAvailable)
                return new SafeIconHandle();

            StockIconInfo info = new StockIconInfo();
            info.Size = Marshal.SizeOf(typeof(StockIconInfo));

            if (_GetStockIconInfo(identifier, StockSmallIcon | StockHandle, ref info) == 0)
                return new SafeIconHandle(info.Handle);

            return new SafeIconHandle();
        }
    }
}
