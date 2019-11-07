////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Gadgeteer.Interfaces
{
    using System;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;
    using Gadgeteer.Modules;

    internal class NativeI2CBus : Socket.SocketInterfaces.I2CBus
    {
        private static I2CDevice _device;
        private I2CDevice.Configuration _configuration;

        public NativeI2CBus(Socket socket, ushort address, int clockRateKhz, Module module)
        {
            if (_device == null)
            {
                socket.ReservePin(Socket.Pin.Eight, module);
                socket.ReservePin(Socket.Pin.Nine, module);

                _device = new I2CDevice(new I2CDevice.Configuration(0, 50));
            }

            _configuration = new I2CDevice.Configuration(address, clockRateKhz);
        }

        public override ushort Address
        {
            get { return _configuration.Address; }
            set { _configuration = new I2CDevice.Configuration(value, this._configuration.ClockRateKhz); }
        }

        public override int Execute(I2CDevice.I2CTransaction[] transactions, int millisecondsTimeout)
        {
            lock (_device)
            {
                _device.Config = _configuration;
                return _device.Execute(transactions, millisecondsTimeout);
            }
        }

        public override void Dispose()
        {
            _device.Dispose();
        }
    }


    // I2C Bus provides support for multiple I2C devices sharing the bus.
    /// <summary>
    /// Represents a device on an Inter-Integrated Circuit (I2C) multi-drop 2-wire bus.
    /// </summary>
    public class I2CBus
    {
        internal readonly Socket.SocketInterfaces.I2CBus Interface;

        /// <summary>
        /// Gets or sets the address of the <see cref="I2CBus"/> device.
        /// </summary>
        public int Address
        {
            get { return this.Interface.Address; }
            set { this.Interface.Address = (ushort)value; }
        }

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <remarks>This automatically checks that the socket supports Type I, and reserves the SDA and SCL pins.
        /// An exception will be thrown if there is a problem with these checks.</remarks>
        /// <param name="address">The address for the I2C device.</param>
        /// <param name="clockRateKhz">The clock rate, in kHz, used when communicating with the I2C device.</param>
        /// <param name="socket">The socket for this I2C device interface.</param>
        /// <param name="module">The module using this I2C interface, which can be null if unspecified.</param>
        public I2CBus(Socket socket, ushort address, int clockRateKhz, Module module)
        {
            socket.EnsureTypeIsSupported('I', module);

            if (socket.I2CBusIndirector != null)
                Interface = socket.I2CBusIndirector(socket, address, clockRateKhz, module);

            else
                Interface = new NativeI2CBus(socket, address, clockRateKhz, module);
        }

        /// <summary>
        /// Writes an array of bytes to the I2C device.
        /// </summary>
        /// <param name="writeBuffer">The array of bytes that will be sent to the I2C device.</param>
        /// <param name="timeout">The amount of time, in milliseconds, that the system will wait before resuming execution of the transaction.</param>
        /// <returns>The number of bytes of data transferred in the transaction.</returns>
        public int Write(byte[] writeBuffer, int timeout)
        {
            var transactions = new I2CDevice.I2CWriteTransaction[] { I2CDevice.CreateWriteTransaction(writeBuffer) };

            return Interface.Execute(transactions, timeout);
        }

        /// <summary>
        /// Reads an array of bytes from the device into a specified read buffer.
        /// </summary>
        /// <param name="readBuffer">The array of bytes that will contain the data read from the I2C device.</param>
        /// <param name="timeout">The amount of time, in milliseconds, that the system will wait before resuming execution of the transaction.</param>
        /// <returns>The number of bytes of data transferred in the transaction.</returns>
        public int Read(byte[] readBuffer, int timeout)
        {
            var transactions = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(readBuffer) };

            return Interface.Execute(transactions, timeout);
        }

        /// <summary>
        ///  Writes an array of bytes to the I2C device, and reads an array of bytes from the device into a specified read buffer.
        /// </summary>
        /// <param name="writeBuffer">The array of bytes that will be sent to the I2C device.</param>
        /// <param name="readBuffer">The array of bytes that will contain the data read from the I2C device.</param>
        /// <param name="timeout">The amount of time, in milliseconds, that the system will wait before resuming execution of the transaction.</param>
        /// <returns>The number of bytes of data transferred in the transaction.</returns>
        public int WriteRead(byte[] writeBuffer, byte[] readBuffer, int timeout)
        {
            var transactions = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(writeBuffer), I2CDevice.CreateReadTransaction(readBuffer) };

            return Interface.Execute(transactions, timeout);
        }

        /// <summary>
        /// Performs a series of I2C transactions. 
        /// </summary>
        /// <remarks>
        /// This is a more advanced API for when <see cref="Write"/>, <see cref="Read"/> and <see cref="WriteRead"/> do not suffice. You may wish to use <see cref="I2CDevice.CreateWriteTransaction"/> and <see cref="I2CDevice.CreateReadTransaction"/> to create the transactions.
        /// </remarks>
        /// <param name="transactions">The list of transactions to perform.</param>
        /// <param name="timeout">The amount of time, in milliseconds, that the system will wait before resuming execution of the transaction.</param>
        /// <returns>The number of bytes successfully transacted.</returns>
        public int Execute(I2CDevice.I2CTransaction[] transactions, int timeout)
        {
            return Interface.Execute(transactions, timeout);
        }

        /// <summary>
        /// Returns the <see cref="Socket.SocketInterfaces.I2CBus" /> for an <see cref="I2CBus" />.
        /// </summary>
        /// <param name="this">An instance of <see cref="I2CBus" />.</param>
        /// <returns>The <see cref="Socket.SocketInterfaces.I2CBus" /> for <paramref name="this"/>.</returns>
        public static explicit operator Socket.SocketInterfaces.I2CBus(I2CBus @this)
        {
            return @this.Interface;
        }

        /// <summary>
        /// Releases all resources used by the interface.
        /// </summary>
        public void Dispose()
        {
            Interface.Dispose();
        }
    }
}