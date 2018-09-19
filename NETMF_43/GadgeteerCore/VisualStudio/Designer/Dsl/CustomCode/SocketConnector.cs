// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Modeling.Diagrams;
using EnvDTE;
using System.Runtime.InteropServices;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Customization of the socket connector
    /// </summary>
    partial class SocketConnector
    {
        public override bool HasToolTip
        {
            get
            {
                return true;
            }
        }

        public override string GetToolTipText(DiagramItem item)
        {
            SocketUseShape fromShape = FromShape as SocketUseShape;
            SocketShape toShape = ToShape as SocketShape;

            if (fromShape == null || toShape == null)
                return base.GetToolTipText(item);

            SocketUse fromSocket = fromShape.ModelElement as SocketUse;
            Socket toSocket = toShape.ModelElement as Socket;

            if (fromShape == null || toShape == null)
                return base.GetToolTipText(item);

            string name = null;

            if (toSocket.GadgeteerHardware is Mainboard)
                name = ((Mainboard)toSocket.GadgeteerHardware).Name;

            else if (toSocket.GadgeteerHardware is Module)
                name = ((Module)toSocket.GadgeteerHardware).Name;

            return string.Format("{0} ({1}) ↔ {2} ({3})", fromSocket.Module.Name, fromSocket.Label, name, toSocket.Label);
        }
    }
}