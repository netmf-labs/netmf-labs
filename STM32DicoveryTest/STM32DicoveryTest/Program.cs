using System;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;
using System.IO.Ports;

namespace STM32DicoveryTest
{
    public class Program
    {
        private static bool _flag;
        private static SerialPort _serial;
        private static bool _adcIsRunning;
        private static OutputPort led = new OutputPort(Pins.GPIO_PIN_D_14, false);

        /* SPI */
        private static SPI spi = new SPI(GetSpiConfig());

        /* LCD */
        private static OutputPort DC_PIN = new OutputPort(Pins.GPIO_PIN_D_13, false);
        private static OutputPort SCE_PIN = new OutputPort(Pins.GPIO_PIN_D_11, false);
        private static OutputPort RST_PIN = new OutputPort(Pins.GPIO_PIN_D_9, false);
        private static LCD lcd;

        /* ADC */
        private static AnalogInput adc;

        /* PWM */
        private static PWM pwm;
        private static RttlPlayer player;

        public static void Main()
        {
            /* Serial console init*/
            _serial = new SerialPort(SerialPorts.COM2, 9600, Parity.None, 8, StopBits.One);
            _serial.Open();
            _serial.DataReceived += serial1_DataReceived;

            byte[] msgInit = Encoding.UTF8.GetBytes("Connection established!\n");
            _serial.Write(msgInit, 10, msgInit.Length);

            /* ADC init*/
            adc = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
            adc.Scale = 100;

            /* LCD init */
            lcd = new LCD(spi, DC_PIN, SCE_PIN, RST_PIN);

            /* PWM init*/
            pwm = new PWM(Cpu.PWMChannel.PWM_4, 50, 0, false);
            player = new RttlPlayer(new PwmSpeaker(pwm));

            while (true)
            {
                /* Loop forever */

                if (_adcIsRunning)
                {
                    int val = (int)adc.Read();
                    byte[] adc_string = Encoding.UTF8.GetBytes(val.ToString());
                    _serial.Write(adc_string, 0, adc_string.Length);
                }
            }
        }


        private static SPI.Configuration GetSpiConfig()
        {
            SPI.Configuration spiConfig = new SPI.Configuration(
                Cpu.Pin.GPIO_NONE, // SS
                false, // SS-active state
                0, // SS startup delay
                0, // SS hold time
                false, // Clock idle state
                true, // Sample on clock edge
                20000, // SPI@20kHz
                SPI.SPI_module.SPI1 // SPI bus
                );
            return spiConfig;
        }


        private static void serial1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesCount = _serial.BytesToRead;
            char cmd1 = '0';
            char cmd2 = '0';
            if (bytesCount > 0)
            {
                byte[] buf = new byte[bytesCount];
                int len = buf.Length;
                _serial.Read(buf, 0, len);
                for (int i = 0; i < len; i++)
                {
                    if (i + 1 < len && buf[i] == (byte)'A' && buf[i + 1] == (byte)'B')
                    {
                        int o1 = i + 2;
                        int o2 = i + 3;
                        if (o1 < len && o2 < len)
                        {
                            cmd1 = Convert.ToChar(buf[o1]);
                            cmd2 = Convert.ToChar(buf[o2]);
                            break;
                        }
                    }
                }

                switch (cmd1)
                {
                    case '1':
                        /* GPIO */
                        _flag = !_flag;
                        led.Write(_flag);
                        break;
                    case '2':
                        /* ADC */
                        _adcIsRunning = !_adcIsRunning;
                        break;
                    case '3':
                        /* PWM */
                        foreach (string song in Songs.MidiList)
                            player.Play(song);
                        pwm.Stop();
                        break;
                    case '4':
                        /* SPI */
                        lcd.Clear();
                        lcd.WriteBlock(LcdImage.duck);
                        break;
                    case '5':
                        switch (cmd2)
                        {
                            case 'U':
                                lcd.WriteBlock(LcdImage.up_img);
                                break;
                            case 'D':
                                lcd.WriteBlock(LcdImage.down_img);
                                break;
                            case '\0':
                                lcd.WriteBlock(LcdImage.na_img);
                                break;
                        }
                        break;
                }
            }
        }
    }
}