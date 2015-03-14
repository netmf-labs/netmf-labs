using System.Threading;
using Microsoft.SPOT.Hardware;

namespace STM32DicoveryTest
{
    /// <summary>
    /// Represents Phillips 84x48 PCD8544 display driver
    /// </summary>
    public class LCD
    {
        private OutputPort DC_PIN, SCE_PIN, RST_PIN;
        private SPI spi;

        /// <summary>
        /// Initializes a new instance of the LCD class
        /// </summary>
        /// <param name="spi"></param>
        /// SPI bus on which LCD is connected
        /// <param name="DC_PIN"></param>
        /// Data/command pin
        /// <param name="SCE_PIN"></param>
        /// SCE pin
        /// <param name="RST_PIN"></param>
        /// Reset pin
        public LCD (SPI spi, OutputPort DC_PIN, OutputPort SCE_PIN, OutputPort RST_PIN)
        {
            this.DC_PIN = DC_PIN;
            this.SCE_PIN = SCE_PIN;
            this.RST_PIN = RST_PIN;
            this.spi = spi;
            Initialize();
        }

        private void Initialize()
        {
            // Enable LCD writing
            SCE_PIN.Write(false);
            // Reset LCD
            RST_PIN.Write(false);
            // Allow the LCD controller some time to reset
            Thread.Sleep(100);
            // Disable reset
            RST_PIN.Write(true);
             // Disable LCD writing
            SCE_PIN.Write(true);
            byte[] initCommands = new byte[] { 0x21, 0xE0, 0x04, 0x13, 0x20, 0x0C };
            foreach (byte command in initCommands)
            {
                SendCommand(command);
            }
            Clear();
        }

        public void Clear()
        {
            GoTo(0, 0);
            for (int i = 0; i < 6; ++i)
                for (int j = 0; j < 84; ++j)
                    WriteByte(0x00);
            GoTo(0, 0);
        }

        public void WriteByte(byte b)
        {
            SCE_PIN.Write(false);
            DC_PIN.Write(true);
            spi.Write(new[] { b });
            SCE_PIN.Write(true);
        }

        public void GoTo(byte x, byte y)
        {
            SendCommand((byte)(0x80 | x));
            SendCommand((byte)(0x40 | y));
        }

        public void WriteBlock(byte[] block)
        {
            SCE_PIN.Write(false);
            DC_PIN.Write(true);
            foreach (byte b in block)
            {
                spi.Write(new[] { b });
            }
            SCE_PIN.Write(true);
        }

        public void SendCommand(byte command)
        {
            SCE_PIN.Write(false);
            DC_PIN.Write(false);
            spi.Write(new[] { command });
            SCE_PIN.Write(true);
        }

    }
}
