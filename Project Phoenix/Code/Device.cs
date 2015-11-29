using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;

namespace Project_Phoenix.Code
{
    public class Device
    {
        private static BluetoothSerial bluetooth;
        private static UsbSerial usb;
        public static RemoteDevice Arduino;
        public static bool UseUSB { get; set; }
        public static string DeviceName { get; set; }
        public static int Baud { get; set; }

        public static void Initiate(string deviceName, int baud)
        {
            Baud = baud;
            DeviceName = deviceName;
            if (UseUSB)
            {
                usb = new UsbSerial(deviceName);
                usb.begin((uint)baud, SerialConfig.SERIAL_8N1);
                Arduino = new RemoteDevice(usb);
            }
            else
            {
                bluetooth = new BluetoothSerial(deviceName);
                bluetooth.begin((uint)baud, SerialConfig.SERIAL_8N1);
                Arduino = new RemoteDevice(bluetooth);
            }
        }

        public Device()
        {
            
        }
    }
}
