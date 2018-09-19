// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.VisualStudio.Modeling;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Custom builder for connecting sockets. It allows us to connect in both directions Socket-->SocketUse and
    /// SocketUse-->Socket.
    /// </summary>
    public static partial class SocketUseReferencesSocketBuilder
    {
        internal static bool CanAcceptSource(ModelElement sourceElement)
        {
            // Just checking the types here. Could also check other conditions e.g. multiplicities.
            if (!(sourceElement is Socket || sourceElement is SocketUse))
                return false;

            var socket = sourceElement as Socket;
            if (socket != null && socket.SocketUse == null)
                return true;

            var socketUse = sourceElement as SocketUse;
            if (socketUse != null && socketUse.Socket == null)
                return true;

            return false;
        }

        // Called during drag-drop to determine whether the supplied source and target combination is valid.
        internal static bool CanAcceptSourceAndTarget(ModelElement sourceElement, ModelElement targetElement)
        {
            SocketUse socketUse = sourceElement as SocketUse ?? targetElement as SocketUse;
            Socket socket = sourceElement as Socket ?? targetElement as Socket;

            if (socket == null || socketUse == null)
                return false;

            return socketUse.CanConnectTo(socket);
        }

        // Called on drop to actually create the new relationship.
        internal static void Connect(ModelElement sourceElement, ModelElement targetElement)
        {
            SocketUse socketUse = sourceElement as SocketUse ?? targetElement as SocketUse;
            Socket socket = sourceElement as Socket ?? targetElement as Socket;

            ElementLink newLink = new SocketUseReferencesSocket(socketUse, socket);
            if (DomainClassInfo.HasNameProperty(newLink))
            {
                DomainClassInfo.SetUniqueName(newLink);
            }

        }


    }

}
