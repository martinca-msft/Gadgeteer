// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Class used to implement the "Connect all modules" feature
    /// </summary>
	public static class AutoConnect
	{
        private const int TimeoutSeconds = 10;

        /// <summary>
        /// Performs a depth-first search to find a suitable configuration where all given modules are connected to the mainboard.
        /// If this method fails it'll leave the model in the last partially connected state explored.
        /// </summary>
        /// <returns>True if a valid configuration was found</returns>
		public static bool Solve(IEnumerable<Socket> providedSockets, IEnumerable<SocketUse> consumedSockets)
		{
			// commented out because of the possiblility of optional sockets
			//if (providedSockets.Sockets.Count < consumedSockets.Count())
				//return false;

            //Use a stack to store the unexplored states
			var stack = new Stack<PossibleConnection>();

            if (!FindAndPush(providedSockets, consumedSockets, stack))
                return true;

            DateTime begin = DateTime.Now;
            while (true)
            {
                if (stack.Count == 0)
                    return false;

                var p = stack.Peek();
                if (p.Tried)
                {
                    stack.Pop();
                    p.Socket.SocketUse = null;
                }
                else
                {
                    p.Tried = true;

                    //Do the connection
                    p.Socket.SocketUse = p.SocketUse;

                    if (!FindAndPush(providedSockets, consumedSockets, stack))
                        return true;
                }

                if ((DateTime.Now - begin).TotalSeconds > TimeoutSeconds)
                {
                    var result = MessageBox.Show(Resources.UI.AutoConnectTimeout, Resources.UI.MessageBoxTitle, MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        throw new OperationCanceledException();
                    }
                    else
                    {
                        begin = DateTime.Now;
                    }
                }
            }
        }

        /// <summary>
        /// Finds unexplored states and push them into the stack                            
        /// </summary>
        /// <returns>true if any potential connections were found</returns>
        private static bool FindAndPush(IEnumerable<Socket> providedSockets, IEnumerable<SocketUse> consumedSockets, Stack<PossibleConnection> stack)
        {
            SocketUse m = FindUnconnectedSocketUse(consumedSockets);
            if (m != null)
                PushPosibleConnections(providedSockets, m, stack);
            return m != null;
        }

		private static SocketUse FindUnconnectedSocketUse(IEnumerable<SocketUse> mSockets)
		{
            return (from s 
                    in mSockets 
                    where s.Socket == null 
                    select s).FirstOrDefault();
		}

		private static void PushPosibleConnections(IEnumerable<Socket> sockets, SocketUse socketUse, Stack<PossibleConnection> stack)
		{
			foreach (var socket in sockets)
			{
				if (socket.SocketUse != null)
					continue;

				if(socketUse.CanConnectTo(socket))
					stack.Push(new PossibleConnection() { Socket = socket, SocketUse = socketUse });
			}
		}

	}

    //Simple state class
	public class PossibleConnection
	{
		public bool Tried; //default is false
        public Socket Socket;
        public SocketUse SocketUse;
	}
}
