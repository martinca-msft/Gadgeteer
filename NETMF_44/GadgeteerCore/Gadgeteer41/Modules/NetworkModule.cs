// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
namespace Gadgeteer.Modules
{
    using Microsoft.SPOT;
    using Microsoft.SPOT.Net.NetworkInformation;
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Net;

    public partial class Module
    {

        /// <summary>
        /// Abstract class to provide common methods, properties, and events for a network module.
        /// </summary>
        public abstract class NetworkModule : Module
        {
            /// <summary>
            /// Gets a value that indicates whether the network connection is up.
            /// </summary>
            /// <remarks>
            /// <para>
            ///  When this property is <b>true</b>, the network connection is connected 
            ///  (<see cref="IsNetworkConnected"/> is <b>true</b>) and IP settings are configured (either statically or using DHCP as requested).
            /// </para>
            /// <note>
            ///  When this property is <b>true</b>, it does not necessarily mean 
            ///  that the network connection is functional. The IP configuration
            ///  for the network connection may be invalid for the network that it is connected to.
            /// </note>
            /// </remarks>
            public bool IsNetworkUp { get; protected set; }

            /// <summary>
            /// The network availability thread synchronization object associated with this network.
            /// </summary>
            protected ManualResetEvent NetworkAvailablityBlocking = null;

            private byte[] physicalAddress;

            /// <summary>
            /// Gets a NetworkInterface object that describes the settings associated with this network module.
            /// </summary>
            /// <remarks>
            /// The derived class should set this once on initialization (to disambiguate between multiple network interfaces, by using the physical address provided).
            /// </remarks>
            public NetworkInterface NetworkSettings
            {
                get
                {
                    if (physicalAddress == null) return null;

                    foreach (var netif in NetworkInterface.GetAllNetworkInterfaces())
                        if (IsThisModuleAddress(netif.PhysicalAddress))
                            return netif;

                    return null;
                }

                protected set
                {
                    physicalAddress = value.PhysicalAddress;
                }
            }

            private bool IsThisModuleAddress(byte[] physicalAddress)
            {
                if (physicalAddress == null)
                    return false;

                if (physicalAddress.Length != this.physicalAddress.Length)
                    return false;

                for (int b = 0; b < physicalAddress.Length; b++)
                    if (physicalAddress[b] != this.physicalAddress[b])
                        return false;

                return true;
            }

            /// <summary>
            /// Cached value of whether the network was previously available or not.
            /// </summary>
            protected bool _networkAvailable = false;

            // Note: A constructor summary is auto-generated by the doc builder.
            /// <summary></summary>
            protected NetworkModule()
            {
                NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
                IsNetworkUp = false;
                dhcpThread = new Thread(new ThreadStart(DHCPThread));
                dhcpThread.Start();
            }

            private Thread dhcpThread;

            /// <summary>
            /// Specifies whether this network connection uses the Dynamic Host Configuration Protocol (DHCP).
            /// </summary>
            protected bool _useDHCP = false;

            /// <summary>
            /// Instructs the network connection to use the Dynamic Host Configuration Protocol (DHCP)
            /// to obtain its IP configuration.
            /// </summary>
            public void UseDHCP()
            {
                // set the useDHCP flag to true
                // then set the event
                // this will cause the dhcp thread to wake up 
                // ensure that the useDHCP flag is set first so that there is 
                // no potential race condition
                _useDHCP = true;
                if (IsNetworkConnected) _needDHCP.Set();
            }

            /// <summary>
            /// Instructs the network connection to use a static Internet Protocol (IP) configuration.
            /// </summary>
            /// <param name="IPAddress">The IP address.</param>
            /// <param name="SubnetMask">The subnet mask.</param>
            /// <param name="GatewayAddress">The IP address for the default gateway.</param>
            /// <param name="DnsAddresses">The IP address of each Domain Name Server (DNS) that this network connection can use.</param>
            /// <remarks>
            /// Pass <b>null</b> to <paramref name="DnsAddresses"/> to disable static DNS. 
            /// </remarks>
            public void UseStaticIP(string IPAddress, string SubnetMask, string GatewayAddress, string[] DnsAddresses)
            {
                _useDHCP = false;

                NetworkSettings.EnableStaticIP(IPAddress, SubnetMask, GatewayAddress);

                // should we be passing null to disable this?
                if (DnsAddresses != null)
                {
                    NetworkSettings.EnableStaticDns(DnsAddresses);
                }

                // This compensates for the fact that NetworkChange_NetworkAvailabilityChanged was not firing when
                // using Static IP with an Ethernet module. The assumption here is that if a static IP is requested,
                // and the media is connected, the network can be assumed to be available and up.
                if (IsNetworkConnected)
                {
                    _networkAvailable = true;
                    IsNetworkUp = true;
                    OnNetworkEvent(this, NetworkState.Up);
                }
            }

            /// <summary>
            /// Instructs the network connection to use a static Internet Protocol (IP) configuration.
            /// </summary>
            /// <param name="IPAddress">The IP address.</param>
            /// <param name="SubnetMask">The subnet mask.</param>
            /// <param name="GatewayAddress">The IP address for the default gateway.</param>
            public void UseStaticIP(string IPAddress, string SubnetMask, string GatewayAddress)
            {
                UseStaticIP(IPAddress, SubnetMask, GatewayAddress, null);
            }

            /// <summary>
            /// Gets or sets the dynamic host configuration protocol (DHCP) thread syncronization object associated with this network.
            /// </summary>
            protected AutoResetEvent _needDHCP = new AutoResetEvent(false);

            /// <summary>
            /// Monitors changes in network configuration to enable dynamic host configuration protocol (DHCP) or renew the DHCP lease.
            /// </summary>
            protected void DHCPThread()
            {
                while (true)
                {
                    try { 
                        _needDHCP.WaitOne();
                        if (!_useDHCP) continue;
                        if (!NetworkSettings.IsDhcpEnabled)
                            NetworkSettings.EnableDhcp();
                        else
                        {
                            NetworkSettings.RenewDhcpLease();
                        }
                        IsNetworkUp = true;
                        OnNetworkEvent(this, NetworkState.Up);
                    } catch {
                        ErrorPrint("DHCP Error - networking may not work");
                    }
                }
            }

            /// <summary>
            /// Gets a value that indicates whether this network is connected.
            /// </summary>
            /// <remarks>
            /// Derived classes override this property to return a value that indicates
            /// whether the media state is connected. This does not necessarily mean 
            /// that the network connection is functional.
            /// </remarks>
            abstract public bool IsNetworkConnected { get; }

            private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
            {
                // the event might not be from this network interface, so ignore it if it does not represent the current status of this network module
                if (e.IsAvailable != IsNetworkConnected) return;

                // also ignore the event if this network module is already in that state (so we don't send events twice to users)
                if (e.IsAvailable == _networkAvailable) return;

                if (e.IsAvailable)
                {
                    if (_useDHCP)
                    {
                        _needDHCP.Set();
                    }
                    else
                    {
                        IsNetworkUp = true;
                        OnNetworkEvent(this, NetworkState.Up);
                    }
                }
                else
                {
                    IsNetworkUp = false;
                    OnNetworkEvent(this, NetworkState.Down);
                }
                _networkAvailable = e.IsAvailable;
            }

            private Object _Lock = new Object();

            /// <summary>
            /// Represents the state of a network connection. 
            /// </summary>
            public enum NetworkState
            {
                /// <summary>
                /// The network connection is properly configured 
                /// and able to perform network communication tasks.
                /// </summary>
                Up,
                /// <summary>
                /// The network connection is down. This might indicate an invalid configuration
                /// for the connection or disconnected media.
                /// </summary>
                Down
            }

            /// <summary>
            /// Represents the delegate that is used to raise 
            /// the <see cref="NetworkUp"/> and <see cref="NetworkDown"/> events.
            /// </summary>
            /// <param name="sender">The network module that raised the event.</param>
            /// <param name="state">The state of the network.</param>
            public delegate void NetworkEventHandler(NetworkModule sender, NetworkState state);

            private event NetworkEventHandler _NetworkUp;

            /// <summary>
            /// Raised when network is connected and configured for Internet Proctocol (IP) communication tasks. 
            /// </summary>
            /// <remarks>
            /// <para>
            ///  Handle this event to perform tasks associated with the network state.
            ///  This event is raised when <see cref="NetworkState"/> changes from
            ///  <b>NetworkState.Down</b> to <b>NetworkState.Up</b>.  
            ///  If the network is up when you add an event handler, the event is raised immediately on the new handler.
            /// </para> 
            /// <note>
            ///  When this event is raised, it does not necessarily mean that the network connection is functional. 
            ///  The IP configuration for the network connection may be invalid for the network that it is connected to.
            /// </note>
            /// </remarks>
            public event NetworkEventHandler NetworkUp
            {
                add
                {
                    lock (_Lock)
                    {
                        if (IsNetworkUp)
                        {
                            Program.BeginInvoke(value, this, NetworkState.Up);
                        }
                        _NetworkUp += value;
                    }
                }
                remove
                {
                    lock (_Lock)
                    {
                        _NetworkUp -= value;
                    }
                }
            }

            private event NetworkEventHandler _NetworkDown;

            /// <summary>
            /// Raised when the network connection is not able to perform network communication tasks.
            /// </summary>
            /// <remarks>
            /// Handle this event to perform tasks associated with the network state.
            /// This event is raised when <see cref="NetworkState"/> changes from
            /// <b>NetworkState.Up</b> to <b>NetworkState.Down</b>. 
            /// If the network is down when you add an event handler, the event is raised immediately on the new handler.
            /// </remarks>
            public event NetworkEventHandler NetworkDown
            {
                add
                {
                    lock (_Lock)
                    {
                        if (!IsNetworkUp)
                        {
                            Program.BeginInvoke(value, this, NetworkState.Down);
                        }
                        _NetworkDown += value;
                    }
                }
                remove
                {
                    lock (_Lock)
                    {
                        _NetworkDown -= value;
                    }
                }
            }

            private NetworkEventHandler _OnNetwork;

            /// <summary>
            /// Raises the <see cref="NetworkUp"/> or <see cref="NetworkDown"/> event.
            /// </summary>
            /// <param name="sender">The <see cref="NetworkModule"/> that raised the event.</param>
            /// <param name="state">The state of the network.</param>
            protected virtual void OnNetworkEvent(NetworkModule sender, NetworkState state)
            {
                if (_OnNetwork == null) _OnNetwork = new NetworkEventHandler(OnNetworkEvent);
                if (Program.CheckAndInvoke(state == NetworkState.Up ? _NetworkUp : _NetworkDown, _OnNetwork, sender, state))
                {
                    switch (state)
                    {
                        case NetworkState.Up:
                            _NetworkUp(sender, state);
                            break;
                        case NetworkState.Down:
                            _NetworkDown(sender, state);
                            break;
                    }
                }
            }
        }
    }
}