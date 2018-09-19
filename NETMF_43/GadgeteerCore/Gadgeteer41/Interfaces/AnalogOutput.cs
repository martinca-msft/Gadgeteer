// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using Microsoft.SPOT;
using Gadgeteer.Modules;

namespace Gadgeteer.Interfaces
{
    /// <summary>
    /// Represents analog output on a single pin.
    /// </summary>
    public class AnalogOutput
    {
        /// <summary>
        /// Represents the analog output port.
        /// </summary>
        private Socket socket;

        private Socket.SocketInterfaces.AnalogOutput port;

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <remarks>This automatically checks that the socket supports Type O, and reserves the pin.
        /// An exception will be thrown if there is a problem with these checks.</remarks>
        /// <param name="socket">The analog output capable socket.</param>
        /// <param name="pin">The pin to assign to the analog output.</param>
        /// <param name="module">The module using this analog output interface, which can be null if unspecified.</param>
        public AnalogOutput(Socket socket,  Socket.Pin pin, Module module)
        {
            this.socket = socket;
            socket.EnsureTypeIsSupported('O', module);
                       
            port = socket.AnalogOutput;
            if (port == null)
            {
                // this is a mainboard error but should not happen since we check for this, but it doesnt hurt to double-check
                throw new Socket.InvalidSocketException("Socket " + socket + " has an error with its Analog Output functionality. Please try a different socket.");
            }

            socket.ReservePin(pin, module);
        }

        /// <summary>
        /// Gets the minimum output voltage of the analog output. 
        /// </summary>
        /// <returns>A double value that represents the minimum output voltage.</returns>
        public double MinOutputVoltage
        {
            get
            {
                return this.port.MinOutputVoltage;
            }
        }

        /// <summary>
        /// Gets the maximum output voltage of the analog output.
        /// </summary>
        /// <returns>A double value that represents the maximum output voltage.</returns>
        public double MaxOutputVoltage
        {
            get
            {
                return this.port.MaxOutputVoltage;
            }
        }

        /// <summary>
        /// Gets or sets the active state of the analog output.
        /// </summary>
        /// /// <returns>A Boolean value, true if the analogue output is active, otherwise false.</returns>
        public bool Active
        {
            get
            {
                return this.port.Active;
            }

            set
            {
                this.port.Active = value;
            }
        }

        /// <summary>
        /// Sets the voltage of the analog output.
        /// </summary>
        /// <param name="voltage">A double value that represents the voltage to which
        /// the analogue output will be set.</param>
        public void Set(double voltage)
        {
            if ((voltage < port.MinOutputVoltage) || (voltage > port.MaxOutputVoltage))
            {
                throw new ArgumentException("Voltage is out of range for this analog output interface");
            }
            
            this.port.SetVoltage(voltage);
        }


    }
}
