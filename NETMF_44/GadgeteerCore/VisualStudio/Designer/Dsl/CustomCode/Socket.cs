// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Gadgeteer.Designer.Definitions;
using ModuleDefinition = Microsoft.Gadgeteer.Designer.Definitions.ModuleDefinition;
using ProvidedSocket = Microsoft.Gadgeteer.Designer.Definitions.ProvidedSocket;
using SocketBaseDefinition = Microsoft.Gadgeteer.Designer.Definitions.SocketBase;
using SocketUseDefinition = Microsoft.Gadgeteer.Designer.Definitions.SocketUse;
using System;


namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// A provided socket 
    /// </summary>
    partial class Socket
    {
        ProvidedSocket definition;

		public override bool IsConnected
		{
			get { return this.SocketUse != null; }
		}

		public override void Disconnect()
		{
			this.SocketUse = null;
		}

        private static readonly ProvidedSocket emptyDefinition = new ProvidedSocket()
        {            
            Types = new Collection<string>(),
            Left = 10,
            Top  = 7
        };

        /// <summary>
        /// The associated ProvidedSocket definition
        /// </summary>
        public override SocketBaseDefinition Definition
        {
            get 
            {
                if (definition == null)
                {
                    IEnumerable<ProvidedSocket> sockets = this.GadgeteerHardware == null ? null :
                        (this.GadgeteerHardware.GadgeteerPartDefinition == null ? null :
                        this.GadgeteerHardware.GadgeteerPartDefinition.ProvidedSockets);

                    if (sockets == null) return emptyDefinition;
                    definition = (from s in sockets where s.Label == this.Label select s).FirstOrDefault();

                    if (definition == null)
                        return emptyDefinition;
                }
                return definition;
            }
        }

        public override bool HasDefinition
        {
            get { return definition != null; }
        }
        
        /// <summary>
        /// Whether this socket can connect to the given one. This checks the socket types and pin conflicts.
        /// Used to highlight and allow/disallow during connect operations and by the AutoConnect class
        /// </summary>
        /// <param name="targetSocket"></param>
        /// <returns></returns>
        internal override bool CanConnectTo(SocketBase targetSocket)
        {
 	        var target = targetSocket as SocketUse;
            if (target == null)return false;

            return target.CanConnectTo(this);
        }
    }
    
    partial class SocketUse
    {
        SocketUseDefinition definition;
        private static SocketUseDefinition emptyDefinition = new SocketUseDefinition()
        {
            Types = new Collection<SocketType>(),
            Pins = new Collection<Pin>()            
        };

		public override bool IsConnected
		{
			get { return this.Socket != null; }
		}

        //Calculated "Optional" property
        internal bool GetOptionalValue()
        {
            var def = Definition as SocketUseDefinition;
            if(def==null)
                return false;

            return def.Optional;
        }

		public override void Disconnect()
		{
			this.Socket = null;
		}
		
		/// <summary>
        /// The associated Definitions.SocketUse
        /// </summary>
        public override SocketBaseDefinition Definition
        {
            get
            {
                if (definition == null)
                {
                    IEnumerable<SocketUseDefinition> sockets = this.Module == null ? null :
                        (this.Module.GadgeteerPartDefinition == null ? null :
                        ((ModuleDefinition)this.Module.GadgeteerPartDefinition).Sockets);

                    if (sockets == null) return emptyDefinition; 

                    definition = (from s in sockets where s.TypesLabel == this.Label select s).FirstOrDefault();

                    if (definition == null)
                    {
                        definition = (from s in sockets where s.UniqueLabel == this.Label select s).FirstOrDefault();

                        if (definition == null)
                            return emptyDefinition;
                    }
                }
                return definition;
            }
        }

        public override bool HasDefinition
        {
            get { return definition != null; }
        }
        
        /// <summary>
        /// Whether this socket can connect to the given one. This checks the socket types and pin conflicts.
        /// Used to highlight and allow/disallow during connect operations and by the AutoConnect class
        /// </summary>
        internal override bool CanConnectTo(SocketBase targetSocket)
        {
            if (this.IsConnected)
                return false; //No, I'm already connected

            var target = targetSocket as Socket;
            if (target == null)return false;

            if (target.IsConnected)
                return false; //No, target is already connected

            if (this.Module == target.GadgeteerHardware)
                return false; //Disallow connections onto the same module
            

            var providedSocket = target.Definition as ProvidedSocket;
            if (providedSocket == null)
            {
                Log.WriteError("Could not find the ProvidedSocket definition for socket {0} in module {1}", target.Label, target.GadgeteerHardware);
                return false;
            }

            var socketUseDef = this.Definition as Definitions.SocketUse;
            if (socketUseDef == null)
            {
                Log.WriteError("Could not find the SocketUse definition for socket {0} in module {1}", this.Label, this.Module.Name);
                return false;
            }

			return CanTypeConnect(providedSocket, socketUseDef) && CanPinsConnect(target, this);
        }

        /// <summary>
        /// Checks whether the given ProvidedSocket and SocketUse definitions have compatible socket types
        /// </summary>       
		internal static bool CanTypeConnect(ProvidedSocket providedSocket, Definitions.SocketUse socketUseDef)
		{
			if (providedSocket.Types == null || socketUseDef.Types == null)
				return false;

			foreach (SocketType t in socketUseDef.Types)
			{
				var st = t as SingleSocketType;
				if (st != null)
				{
					if (providedSocket.Types.Any(pst => pst == st.Value))
						return true;
				}
				var ct = t as CompositeType;
				if (ct != null)
				{
					//Can all the types in the composite type by found in the provided socket?
					if (ct.Types.All(cst => providedSocket.Types.Any(pst => pst == cst.Value)))
						return true;
				}
			}

			return false;
		}

        /// <summary>
        /// Checks whether it's possible to connect the given sockets without incurring shared pin conflicts. This method will consider
        /// all other sockets currently connected to the mainboard when checking for conflicts
        /// </summary>        
		internal static bool CanPinsConnect(Socket socket, SocketUse socketUse)
		{
			var providedSocket = socket.Definition as ProvidedSocket;
			if (providedSocket == null)
				return false;

			if (providedSocket.SharedPinMaps == null || providedSocket.SharedPinMaps.Count == 0)
				return true;


			foreach (var sharedPinMap in providedSocket.SharedPinMaps)
			{
				//is it used by the connecting socketuse?
			    SharedPinMap map = sharedPinMap;
			    Pin connectingPin = (from pin
                                     in ((SocketUseDefinition)socketUse.Definition).Pins
                                     where pin.Value == map.SocketPin
                                     select pin).FirstOrDefault();

				if (connectingPin == null)
					continue;

                // if the pin can only be used shared and this use is not compatible with sharing, fail
//#warning this check may be insufficient - have to follow the chain up to see if any provider says "sharedonly"
                if (!connectingPin.Shared && sharedPinMap.SharedOnly)
                    return false;

                // iterate over 

				foreach (var s in socket.GadgeteerHardware.Sockets)
				{
                    //We are interested in sockets other than "us"
					if (s == socket)
						continue;

					if(!s.IsConnected)
						continue;
                                         //socket.GadgeteerHardware.
//#warning need to check USED sockets too, and follow the net downstream
					var otherProvidedSocket = s.Definition as ProvidedSocket;

                    //If there are no shared pin maps there's nothing else to check
					if (otherProvidedSocket.SharedPinMaps == null || otherProvidedSocket.SharedPinMaps.Count == 0)
						continue;

					foreach (var spm in otherProvidedSocket.SharedPinMaps)
					{
                        //are the shared pind maps for the same mainboard resource?
						if (spm.NetId != sharedPinMap.NetId)
							continue;

						//is it in use or are they both shared?
						foreach (var pin in ((SocketUseDefinition)s.SocketUse.Definition).Pins)
						{
							if (pin.Value == spm.SocketPin && (!pin.Shared || !connectingPin.Shared))
								return false;
						}

					}
				}
			}

			return true;
		}

	}

    /// <summary>
    /// Base class for sockets (base for both provided and consumer sockets)
    /// </summary>
    partial class SocketBase
    {
        public abstract bool HasDefinition { get;}
        public abstract SocketBaseDefinition Definition { get; }        
		public abstract bool IsConnected { get; }
		public abstract void Disconnect();

        //Having this in the base class allows us to write the socket highlighting code in a neutral way
        internal abstract bool CanConnectTo(SocketBase target);
    }
}
