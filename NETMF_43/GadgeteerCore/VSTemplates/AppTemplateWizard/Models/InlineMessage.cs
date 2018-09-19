// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal enum InlineSeverity
    {
        None,
        Information,
        Warning,
        Error
    }

    internal class InlineMessage
    {
        public InlineSeverity Severity { get; private set; }
        public TextBlock Message { get; private set; }

        public InlineMessage(InlineSeverity severity, params object[] inlines)
        {
            Severity = severity;

            Message = new TextBlock();
            Message.TextWrapping = TextWrapping.WrapWithOverflow;
            Message.Inlines.AddRange(from inline in inlines
                                     select inline is Inline ? inline : new Run(inline.ToString()));
        }

        private static int GetSeverityIconID(InlineSeverity severity)
        {
            switch (severity)
            {
                case InlineSeverity.Information: return 79;
                case InlineSeverity.Warning: return 78;
                case InlineSeverity.Error: return 80;
                default: return -1;
            }
        }
        public ImageSource SeverityImage
        {
            get
            {
                int iconID = GetSeverityIconID(Severity);

                if (iconID == -1)
                    return null;

                try
                {
                    using (SafeHandle hIcon = NativeMethods.GetStockSmallIconHandle(iconID))
                    {
                        if (hIcon.IsInvalid)
                            return null;

                        return Imaging.CreateBitmapSourceFromHIcon(hIcon.DangerousGetHandle(), Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch { return null; }
            }
        }
    }
}
