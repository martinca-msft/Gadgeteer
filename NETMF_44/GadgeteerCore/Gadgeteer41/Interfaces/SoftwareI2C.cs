// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
namespace Gadgeteer.Interfaces
{
    using System;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;
    using Gadgeteer.Modules;
    using System.Threading;
    using System.Collections;

    /// <summary>
    /// Supports software I2C on any GPIO pins using socket type X or Y. 
    /// </summary>
    /// <remarks>
    /// The module must include pull up resistors in the range 2.2kohm to 10kohm.  Hardware I2C (Type I, using Gadgeteer.Interfaces.I2CBus) modules are not cross-compatible with SoftwareI2C
    /// unless they include switchable pull-ups (and the module code switches them in if it detects Type I is not present).
    /// If the mainboard provides native code support for software I2C, this will be used, otherwise a managed code implementation of I2C will be used.
    /// Only "standard mode" is supported (100 kbit/s maximum, though the speed may be slower). For a fast interface, use hardware I2C (Gadgeteer.Interfaces.I2CBus) or another bus type.
    /// </remarks>
    public class SoftwareI2C
    {
        private static Hashtable ReservedSdaPinPorts = new Hashtable();
        private static Hashtable ReservedSclPinPorts = new Hashtable();

        private TristatePort sclPort;
        private TristatePort sdaPort;

        private Socket socket;
        private Socket.Pin sdaPin, sclPin;

        // Variables set to provide a timeout of about 1 second (for managed code implementation)
        private const int timeoutLoopCount = 10;        // 10 loops of
        private const int timeoutLoopDelay = 100;       // 100 mS each

        private static Thread timeoutThread = null;
        private static bool threadExit;
        private int timeoutCount;        // Create this as an array so that it may be locked
        private bool timeout;            // Set true to abort device communications in the event of a timeout
        private static ArrayList SoftwareI2CTimeoutList = new ArrayList();

        /// <summary>
        /// Defines whether new SoftwareI2C modules will be forced to use the managed implementation (slower) or will be allowed to use the native software I2C implementation if available on the mainboard (faster).  Default is false (native allowed).
        /// </summary>
        public static bool ForceManagedSoftwareI2CImplementation { get; set; }

        /// <summary>
        /// Defines whether new SoftwareI2C modules will use processor pull ups on the IOs (useful if modules omit the mandatory pull ups).  Default is false (modules should provide pull ups).
        /// </summary>
        public static bool ForceManagedPullUps { get; set; }

        static SoftwareI2C()
        {
            ForceManagedPullUps = false;
            ForceManagedSoftwareI2CImplementation = false;
        }

        private bool usingManaged = false;

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <remarks>This automatically checks that the socket supports Type X or Y as appropriate, and reserves the SDA and SCL pins.
        /// An exception will be thrown if there is a problem with these checks.</remarks>
        /// <param name="socket">The socket for this software I2C device interface.</param>
        /// <param name="sdaPin">The socket pin used for I2C data.</param>
        /// <param name="sclPin">The socket pin used for I2C clock.</param>
        /// <param name="module">The module using this I2C interface, which can be null if unspecified.</param>
        public SoftwareI2C(Socket socket, Socket.Pin sdaPin, Socket.Pin sclPin, Module module)
        {
            // first check the socket is compatible
            if (sclPin > Socket.Pin.Five || sdaPin > Socket.Pin.Five)
            {
                socket.EnsureTypeIsSupported('Y', module);
            }
            else
            {
                socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, module);
            }

            if (ForceManagedSoftwareI2CImplementation || socket.NativeI2CWriteRead == null) usingManaged = true;

            // then see if we've already reserved the pins and got instances of the ports, otherwise do that.
            string sdaPinString = socket.ToString() + "___" + sdaPin;
            if (!ReservedSdaPinPorts.Contains(sdaPinString))
            {
                Cpu.Pin sdaCpuPin = socket.ReservePin(sdaPin, module);
                if (usingManaged)
                {
                    sdaPort = new TristatePort(sdaCpuPin, false, false, ForceManagedPullUps ? Port.ResistorMode.PullUp : Port.ResistorMode.Disabled);
                }
                ReservedSdaPinPorts.Add(sdaPinString, sdaPort);
            }
            else
            {
                sdaPort = (TristatePort)ReservedSdaPinPorts[sdaPinString];
            }

            string sclPinString = socket.ToString() + "___" + sclPin;
            if (!ReservedSclPinPorts.Contains(sclPinString))
            {
                Cpu.Pin sclCpuPin = socket.ReservePin(sclPin, module);
                if (usingManaged)
                {
                    sclPort = new TristatePort(sclCpuPin, false, false, ForceManagedPullUps ? Port.ResistorMode.PullUp : Port.ResistorMode.Disabled);
                }
                ReservedSclPinPorts.Add(sclPinString, sclPort);
            }
            else
            {
                sclPort = (TristatePort)ReservedSclPinPorts[sclPinString];
            }

            this.socket = socket;
            this.sdaPin = sdaPin;
            this.sclPin = sclPin;

            if (usingManaged)
            {
                lock (SoftwareI2CTimeoutList)
                {
                    timeoutCount = -1;       // Prevent the TimeoutHandler thread from watching this port for now
                    SoftwareI2CTimeoutList.Add(this);

                    if (timeoutThread == null)
                    {
                        threadExit = false;
                        timeoutThread = new Thread(new ThreadStart(TimeoutHandler));
                        timeoutThread.Start();
                    }
                }
            }

        }

        /// <summary>
        /// This specifies possible options for handling length errors, i.e. when the specified number of bytes cannot be read or written.
        /// Even if these are suppressed, exceptions may still be thrown for bus errors, e.g. if there is a timeout because the clock line is held low too long.
        /// </summary>
        public enum LengthErrorBehavior
        {
            /// <summary>
            /// Throw an exception if the right number of bytes is not written/read.
            /// </summary>
            ThrowException,

            /// <summary>
            /// Suppress exceptions if the right number of bytes is not written/read.
            /// </summary>
            SuppressException
        }

        /// <summary>
        /// Writes an array of bytes and then reads an array of bytes from/to an I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device (bottom 7 bits only).</param>
        /// <param name="writeBuffer">The array of data to write to the device..</param>
        /// <param name="writeOffset">The index of the first byte in the "writeBuffer" array to be written.</param>
        /// <param name="writeLength">The number of bytes from the "writeBuffer" array to be written.</param>
        /// <param name="readBuffer">The array that will hold data read from the device.</param>
        /// <param name="readOffset">The index of the first location in the "readBuffer" array to be written to.</param>
        /// <param name="readLength">The number of bytes that will be written to the "readBuffer" array.</param>
        /// <param name="numWritten">The number of bytes actually written to the device.</param>
        /// <param name="numRead">The number of bytes actually read from the device.</param>
        /// <param name="errorBehavior">Whether or not to throw an exception if the write/read does not complete.</param>
        public void WriteRead(byte address, byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead, LengthErrorBehavior errorBehavior = LengthErrorBehavior.ThrowException)
        {
            // Sanity check the passed arguments
            if (writeBuffer == null)
            {
                writeBuffer = new byte[0];
                writeOffset = 0;
                writeLength = 0;
            }
            else if (writeBuffer.Length < writeOffset + writeLength || writeOffset < 0 || writeLength < 0)
            {
                throw new ArgumentException("SoftwareI2C: WriteRead call to device at address " + address + " on socket " + socket + " has bad writeBuffer parameters (buffer too small or negative length or offset specified)");
            }

            if (readBuffer == null)
            {
                readBuffer = new byte[0];
                readOffset = 0;
                readLength = 0;
            }
            else if (readBuffer.Length < readOffset + readLength || readOffset < 0 || readLength < 0)
            {
                throw new ArgumentException("SoftwareI2C: WriteRead call to device at address " + address + " on socket " + socket + " has bad readBuffer parameters (buffer too small or negative length or offset specified)");
            }

            if (!usingManaged)
            {
                socket.NativeI2CWriteRead(socket, sdaPin, sclPin, address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }
            else
            {
                DoManagedWriteRead(address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }

            if (errorBehavior == LengthErrorBehavior.ThrowException && (numWritten != writeLength || numRead != readLength))
            {
                throw new ApplicationException("SoftwareI2C: Exception writing to device at address " + address + " on socket " + socket + " - perhaps device is not responding or not plugged in.");
            }
        }


        /// <summary>
        /// Writes an array of bytes and then reads an array of bytes from/to an I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device (bottom 7 bits only).</param>
        /// <param name="writeBuffer">The array of data to write to the device.</param>
        /// <param name="readBuffer">The array that will hold data read from the device.</param>
        /// <param name="errorBehavior">Whether or not to throw an exception if the write/read does not complete.</param>
        /// <returns>The total number of bytes transferred in the transaction.</returns>
        public int WriteRead(byte address, byte[] writeBuffer, byte[] readBuffer, LengthErrorBehavior errorBehavior = LengthErrorBehavior.ThrowException)
        {
            int numWritten, numRead;
            WriteRead(address, writeBuffer, 0, writeBuffer == null ? 0 : writeBuffer.Length, readBuffer, 0, readBuffer == null ? 0 : readBuffer.Length, out numWritten, out numRead, errorBehavior);
            if (numWritten < 0) return numWritten;
            if (numRead < 0) return numRead;
            return numWritten + numRead;
        }

        /// <summary>
        /// Writes an array of bytes to an I2C device.
        /// </summary>
        /// <param name="address">The address of the I2C device.</param>
        /// <param name="writeBuffer">The array of bytes that will be written to the I2C device.</param>
        /// <param name="errorBehavior">Whether or not to throw an exception if the write/read does not complete.</param>
        /// <returns>The number of bytes written to the device.</returns>
        public int Write(byte address, byte[] writeBuffer, LengthErrorBehavior errorBehavior = LengthErrorBehavior.ThrowException)
        {
            if (writeBuffer == null) return 0;
            int numWritten, numRead;
            WriteRead(address, writeBuffer, 0, writeBuffer.Length, null, 0, 0, out numWritten, out numRead, errorBehavior);
            return numWritten;
        }

        /// <summary>
        /// Reads a register from a I2C device using a memory map API.
        /// </summary>
        /// <param name="address">The address of the I2C device.</param>
        /// <param name="register">The single byte to write to the device (normally the register address on the device).</param>
        /// <param name="errorBehavior">Whether or not to throw an exception if the read does not complete.</param>
        /// <returns>The single byte read from the device.</returns>
        public byte ReadRegister(byte address, byte register, LengthErrorBehavior errorBehavior = LengthErrorBehavior.ThrowException)
        {
            byte[] toWrite = new byte[1] { register };
            byte[] toRead = new byte[1] { 0 };
            int numWritten, numRead;

            WriteRead(address, toWrite, 0, 1, toRead, 0, 1, out numWritten, out numRead, errorBehavior);

            return toRead[0];
        }

        /// <summary>
        /// Reads an array of bytes from an I2C device.
        /// </summary>
        /// <param name="address">The address of the I2C device.</param>
        /// <param name="readBuffer">The array of bytes that will be read from the I2C device.</param>
        /// <param name="errorBehavior">Whether or not to throw an exception if the read does not complete.</param>
        /// <returns>The number of bytes read from the device.</returns>
        public int Read(byte address, byte[] readBuffer, LengthErrorBehavior errorBehavior = LengthErrorBehavior.ThrowException)
        {
            if (readBuffer == null) return 0;
            int numWritten, numRead;
            WriteRead(address, null, 0, 0, readBuffer, 0, readBuffer.Length, out numWritten, out numRead, errorBehavior);
            return numRead;
        }


        #region Managed Software I2C Implementation

        private static void TimeoutHandler()
        {
            while (!threadExit)
            {
                try
                {
                    // Perform a timeout check after every timeoutLoopDelay
                    Thread.Sleep(timeoutLoopDelay);

                    lock (SoftwareI2CTimeoutList)
                    {
                        // Check each SoftwareI2C instance for non-responsive communications
                        foreach (SoftwareI2C softwarei2c in SoftwareI2CTimeoutList)
                        {
                            if (softwarei2c.timeoutCount >= 0)
                            {
                                softwarei2c.timeoutCount++;
                                if (softwarei2c.timeoutCount >= timeoutLoopCount)
                                {
                                    softwarei2c.timeout = true;              // Abort the current transfer
                                    softwarei2c.timeoutCount = -1;        // This port has failed - no need to keep watching it
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("Exception in SoftwareI2C timeout handling: " + ex);
                }
            }
        }

        // this relies on argument checking already being done in WriteRead
        private void DoManagedWriteRead(byte address, byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead)
        {
            lock (sdaPort)
            {
                lock (SoftwareI2CTimeoutList)
                {
                    timeout = false;
                    timeoutCount = 0;        // Enable timeout checking for this port
                }

                numWritten = 0;
                numRead = 0;

                if (writeLength != 0)
                {
                    // The clock pin should be pulled high - if not, there is a short on the bus
                    // or a nonresponsive device, etc.
                    if (!sclPort.Read()) throw new ApplicationException("Software I2C: clock signal on socket " + socket + " is being held low.");

                    // Generate the start pulse
                    sdaPort.Active = true;
                    sclPort.Active = true;

                    // Write the address and data bytes to the device (low order address bit is 0 for write)
                    if (WriteByte((byte)(address << 1)))
                    {
                        for (int index = writeOffset; index < writeOffset + writeLength; index++)
                        {
                            if (!WriteByte(writeBuffer[index])) break;
                            numWritten++;
                        }
                    }

                    if (readLength == 0 || numWritten != writeLength)
                    {
                        // send stop pulse if not reading, or if write failed
                        sclPort.Active = false;                    // Allow clock pin to float high
                        while (!sclPort.Read() && !timeout) ;      // Wait for the clock pin to go high
                        sdaPort.Active = false;                     // Allow data pin to float high
                    }
                    else
                    {
                        // set up for repeated start condition;
                        sdaPort.Active = false;
                        while (!sdaPort.Read() && !timeout) ;
                        sclPort.Active = false;
                    }
                }

                if (timeout) throw new ApplicationException("Software I2C: clock signal on socket " + socket + " is being held low.");

                if (numWritten == writeLength && readLength != 0)
                {
                    int limit = (readOffset + readLength) - 1;

                    // The clock pin should be pulled high
                    // If it is not, the bus is shorted or a device is nonresponsive
                    if (!sclPort.Read()) throw new ApplicationException("Software I2C: clock signal on socket " + socket + " is being held low.");

                    // Generate the start pulse
                    sdaPort.Active = true;
                    sclPort.Active = true;

                    // Write the address and then read the data bytes from the device (low order address bit is 1 for read)
                    if (WriteByte((byte)((address << 1) | 1)))
                    {
                        int lastIndex = readOffset + readLength - 1;
                        for (int index = readOffset; index < readOffset + readLength; index++)
                        {
                            if (!ReadByte(index == lastIndex, out readBuffer[index])) break;
                            numRead++;
                        }
                    }

                    // Generate the stop pulse
                    sclPort.Active = false;               // Release the clock line
                    while (!sclPort.Read() & !timeout) ;   // Wait for the clock line to go high
                    sdaPort.Active = false;               // Release the data line
                }

                if (timeout) throw new ApplicationException("Software I2C: clock signal on socket " + socket + " is being held low.");

                lock (SoftwareI2CTimeoutList)
                {
                    timeoutCount = -1;       // Disable timeout checking for this port
                }
            }

        }

        private bool WriteByte(byte data)
        {
            bool writtenOK = false;
            byte lastBit = 0;            // Assume that the data pin is currently driving low
            byte nextBit;

            // This routine assumes that the clock and data are both driving low
            // Data is transmitted MSB first
            for (int bit = 0; bit < 8; bit++)
            {
                nextBit = (byte)(data & 0x80);
                if (nextBit != lastBit)     // If the state of the data line must change
                {
                    if (nextBit == 0)      // If the data pin must now drive low
                    {
                        sdaPort.Active = true;
                    }
                    else
                    {
                        sdaPort.Active = false;
                    }
                    lastBit = nextBit;
                }
                data <<= 1;
                sclPort.Active = false;                    // Let clock line go high
                while (!sclPort.Read() && !timeout) ;      // Wait until the clock line actually goes high
                sclPort.Active = true;                     // Drive the clock back low
            }

            // Check the acknowledge (9th) bit
            if (lastBit == 0)
            {
                sdaPort.Active = false;                 // Change the data pin to an input (stop driving it if it was being driven)
            }
            sclPort.Active = false;                    // Let the clock line go high
            while (!sclPort.Read() && !timeout) ;      // Wait for the clock line to actually go high
            writtenOK = !sdaPort.Read();                // The data line must be driven low by the device to acknowledge the data
            sclPort.Active = true;                     // Drive the clock back low

            // Leave the data pin in an expected state (low)
            sdaPort.Active = true;

            if (writtenOK && !timeout)
                lock (SoftwareI2CTimeoutList)
                {
                    timeoutCount = 0;                   // We wrote a byte successfully, reset the timouet
                    return true;
                }

            return false;
        }

        private bool ReadByte(bool last, out byte data)
        {
            data = 0;

            // This routine assumes that the clock and data lines are being driven low
            // Data is received MSB first
            sdaPort.Active = false;                         // Allow the data pin to be driven by the device
            for (int bit = 0; bit < 8; bit++)
            {
                data <<= 1;
                sclPort.Active = false;                    // Let the clock pin go high
                while (!sclPort.Read() && !timeout) ;      // Wait until the device releases the clock pin (if necessary)
                if (sdaPort.Read())
                {
                    data |= 1;
                }
                sclPort.Active = true;                     // Drive the clock pin low again
            }

            // Set the acknowledge bit low for all except the last byte
            if (!last) sdaPort.Active = true;
            sclPort.Active = false;                    // Let the clock pin go high
            while (!sclPort.Read() && !timeout) ;      // Wait for the clock pin to actually go high
            sclPort.Active = true;                     // Drive the clock pin back low
            if (last) sdaPort.Active = true;           // Drive the data pin low again so that it is in an expected state

            if (!timeout)
                lock (SoftwareI2CTimeoutList)
                {
                    timeoutCount = 0;                  // We read a byte successfully, reset the timeout
                    return true;
                }

            return false;
        }
        #endregion
    }
}


