// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
namespace Gadgeteer.Interfaces
{
    using System;
    using Microsoft.SPOT;
    using Gadgeteer.Modules;
    using Microsoft.SPOT.Hardware;

    /// <summary>
    /// Represents analog input on a single pin.
    /// </summary>
    public class AnalogInput
    {
        private Microsoft.SPOT.Hardware.AnalogInput ain = null;
        private Cpu.AnalogChannel channel = Cpu.AnalogChannel.ANALOG_NONE;
        private Socket socket;
        
        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <remarks>This automatically checks that the socket supports Type A, and reserves the pin used.
        /// An exception will be thrown if there is a problem with these checks.</remarks>
        /// <param name="socket">The socket.</param>
        /// <param name="pin">The analog input pin to use.</param>
        /// <param name="module">The module using the socket, which can be null if unspecified.</param>
        public AnalogInput(Socket socket, Socket.Pin pin, Module module) 
        {
            this.socket = socket;
            socket.EnsureTypeIsSupported('A', module);

            switch (pin)
            {
                case Socket.Pin.Three:
                    channel = socket.AnalogInput3;
                    break;
                case Socket.Pin.Four:
                    channel = socket.AnalogInput4;
                    break;
                case Socket.Pin.Five:
                    channel = socket.AnalogInput5;
                    break;
                default:
                    if (module != null)
                    {
                        throw new Socket.InvalidSocketException("Module " + module + " cannot use Analog Input interface on pin " + pin + " - pin must be in range 3 to 5.");
                    }
                    else
                    {
                        throw new Socket.InvalidSocketException("Cannot use Analog Input interface on pin " + pin + " - pin must be in range 3 to 5.");
                    }
            }

            if (channel == Cpu.AnalogChannel.ANALOG_NONE)
            {
                // this is a mainboard error that should not happen (we already check for it in SocketInterfaces.RegisterSocket) but just in case...
                throw new Socket.InvalidSocketException("Socket " + socket + " has an error with its Analog Input functionality. Please try a different socket.");
            }

            socket.ReservePin(pin, module);
        }

        /// <summary>
        /// Gets or sets the active state of the analog input.
        /// </summary>
        /// /// <returns>A Boolean value, true if the analogue input is active, otherwise false.</returns>
        public bool Active
        {
            get
            {
                return this.ain != null;
            }

            set
            {
                if(Active == value) return;
                if (value)
                {
                    ain = new Microsoft.SPOT.Hardware.AnalogInput(channel, socket.AnalogInputScale, socket.AnalogInputOffset, socket.AnalogInputPrecisionInBits);
                }
                else 
                {
                    ain.Dispose();
                    ain = null;
                }
            }
        }

        /// <summary>
        /// Reads the current analog input value as a voltage between 0 and 3.3V.
        /// </summary>
        /// <returns>The current analog value in volts.</returns>
        public double ReadVoltage()
        {
            Active = true;
            return ain.Read();
        }

        /// <summary>
        /// Reads the current analog input value as a proportion from 0.0 to 1.0 of the maximum value (3.3V).
        /// </summary>
        /// <returns>The analog input value from 0.0-1.0</returns>
        public double ReadProportion()
        {
            Active = true;
            return ain.Read() / 3.3;
        }
    }
}