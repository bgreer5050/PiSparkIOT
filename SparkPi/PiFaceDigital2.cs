using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Devices.Gpio;
using Windows.Graphics;
namespace SparkPi
{
    public class PiFaceEventArgs : EventArgs
    {
        public byte PinMask { get; set; }
        public byte PinState { get; set; }
    }
    public delegate void PiFaceInputChange(object sender, PiFaceEventArgs e);
    public class PiFaceSpiDriver : IDisposable
    {
        // notes reset on the MCP23S17 is tied to 3.3V
        // Block GPA is configured as inputs
        // Block GPB is configured as outputs
        // leave the IOCON.BANK = 0
        private GpioController IoController = null;
        private GpioPin InterruptPin = null;
        private const string SPI_CONTROLLER_NAME = "SPI0"; /* For Raspberry Pi 2, use SPI0 */
        private const Int32 SPI_CHIP_SELECT_LINE = 0; /* Line 0 maps to physical pin number 24 on the Rpi2 */
        private const Int32 INTERRUPT_PIN = 25;
        private const byte SPI_SLAVE_READ = 1;
        private const byte SPI_SLAVE_WRITE = 0;
        private const byte SPI_SLAVE_ID = 0x40;
        private const byte SPI_SLAVE_ADDR = 0; // 0, 1, 2
        private const byte IOCONA = 0xA;
        private const byte IODIRA = 0x00;
        private const byte IODIRB = 0x01;
        private const byte GPPUA = 0x0C;
        private const byte GPPUB = 0x0D;
        private const byte GPIOA = 0x12;
        private const byte GPIOB = 0x13;
        private const byte OLATA = 0x14;
        private const byte OLATB = 0x15;
        private const byte DEFVALB = 0x7;
        private const byte INTCONB = 0xB;
        private const byte GPINTENB = 0x5;
        private const byte INTFB = 0xF;
        private const byte INTCAPB = 0x11;
        private SpiDevice SpiDev = null;
        public event EventHandler<PiFaceEventArgs> PiFaceInterrupt;
        public async Task InitHardware()
        {
            IoController = GpioController.GetDefault();
            InterruptPin = IoController.OpenPin(INTERRUPT_PIN);
            InterruptPin.DebounceTimeout = new TimeSpan(0, 0, 0, 0, 50);
            InterruptPin.Write(GpioPinValue.High);
            InterruptPin.SetDriveMode(GpioPinDriveMode.Input);
            InterruptPin.ValueChanged += Interrupt;
            var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
            settings.ClockFrequency = 10000000; // max frequency for MCSP23S17 is 10Mhz
            settings.Mode = SpiMode.Mode3; // data read on the rising edge - idle high
            string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
            var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);
            SpiDev = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);
            await Task.Delay(20);
            Write(IOCONA, 0x28); // BANK=0, SEQOP=1, HAEN=1 (Enable Addressing) interrupt not configured
            Write(IODIRA, 0x00); // GPIOA As Output
            Write(IODIRB, 0xFF); // GPIOB As Input
            Write(GPPUB, 0xFF); // configure pull ups
            Write(DEFVALB, 0x00); // normally high, only applicable if INTCONB == 0x0xFF
            Write(INTCONB, 0x00); // interrupt occurs upon pin change
                                  //Write(INTCONB, 0xFF); // interrupt occurs when pin values become DEFVALB values
            Write(GPINTENB, 0xFF); // enable all interrupts
            Write(GPIOA, 0x55); // drive all outputs low
        }
        private void Write(byte address, byte data)
        {
            if (SpiDev != null)
            {
                byte[] buffer = new byte[3];
                buffer[0] = SPI_SLAVE_ID | ((SPI_SLAVE_ADDR << 1) & 0x0E) | SPI_SLAVE_WRITE;
                buffer[1] = address;
                buffer[2] = data;
                SpiDev.Write(buffer);
            }
        }
        private byte Read(byte address)
        {
            if (SpiDev != null)
            {
                byte[] buffer = new byte[2];
                buffer[0] = SPI_SLAVE_ID | ((SPI_SLAVE_ADDR << 1) & 0x0E) | SPI_SLAVE_READ;
                buffer[1] = address;
                byte[] read_buffer = new byte[1];
                SpiDev.TransferSequential(buffer, read_buffer);
                return read_buffer[0];
            }
            return 0;
        }
        public void WriteGPA(byte value)
        {
            Write(GPIOA, value);
        }
        public byte ReadGPA()
        {
            return Read(OLATA);
        }
        public byte ReadGPB()
        {
            return Read(GPIOB);
        }
        private void Interrupt(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (PiFaceInterrupt != null)
            {
                PiFaceEventArgs pfea = new PiFaceEventArgs();
                pfea.PinMask = Read(INTFB); // find out what caused the interrupt
                pfea.PinState = Read(INTCAPB); // read the state of the pins
                PiFaceInterrupt(this, pfea);
            }
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (SpiDev != null)
                    SpiDev.Dispose();
                if (InterruptPin != null)
                    InterruptPin.Dispose();
                disposedValue = true;
            }
        }
        ~PiFaceSpiDriver()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}