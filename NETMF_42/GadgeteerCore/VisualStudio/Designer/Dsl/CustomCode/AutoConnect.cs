// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Class used to implement the "Connect all modules" feature
    /// </summary>
	public static class AutoConnect
	{
        /// <summary>
        /// Performs a depth-first search to find a suitable configuration where all given modules are connected to the mainboard.
        /// If this method fails it'll leave the model in the last partially connected state explored.
        /// </summary>
        /// <returns>True if a valid configuration was found</returns>
		public static bool Solve(Mainboard mainboard, IEnumerable<SocketUse> mSockets)
		{
			// commented out because of the possiblility of optional sockets
			//if (mainboard.Sockets.Count < mSockets.Count())
				//return false;

            //Use a stack to store the unexplored states
			var stack = new Stack<PossibleConnection>();

            if (!FindAndPush(mainboard, mSockets, stack))
                return true;

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

                    if (!FindAndPush(mainboard, mSockets, stack))
                        return true;
				}
			}
		}

        /// <summary>
        /// Finds unexplored states and push them into the stack
        /// </summary>
        /// <returns>true if any potential connections were found</returns>
        private static bool FindAndPush(Mainboard mainboard, IEnumerable<SocketUse> mSockets, Stack<PossibleConnection> stack)
        {
            SocketUse m = FindUnconnectedSocketUse(mSockets);
            if (m != null)
                PushPosibleConnections(mainboard.Sockets, m, stack);
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
		public Socket Socket { get; set; }
		public SocketUse SocketUse { get; set; }
	}
}
