using Microsoft.Maker.Serial;
using Project_Phoenix.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Project_Phoenix.Views
{
    public sealed partial class MainView : Page
    {
        public MainView()
        {
            this.InitializeComponent();
            radUSB.Checked += rad_Checked;
            radBluetooth.Checked += rad_Checked;
            radBluetooth.IsChecked = true;
            ViewMessage("", Publics.MessageType.Info);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ViewMessage((string)e.Parameter, Publics.MessageType.Error);
                return;
            }
            if (MainPage.isConnected)
            {
                ViewMessage("You have a connected device. If you would like to change it, just connect to another.", Publics.MessageType.Info);
                btnBasicControl.IsEnabled = btnIDE.IsEnabled = true;
            }
        }

        private async void rad_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rad = (RadioButton)sender;
            lstDevices.ItemsSource = null;
            if (rad == radBluetooth)
            {
                var bleutoothDevices = await BluetoothSerial.listAvailableDevicesAsync();
                lstDevices.ItemsSource = bleutoothDevices;
            }
            else if (rad == radUSB)
            {
                var usbDevices = await UsbSerial.listAvailableDevicesAsync();
                List<DeviceInformation> _usbDevices = new List<DeviceInformation>();
                foreach (var itm in usbDevices) if (itm.Name.Contains("COM")) _usbDevices.Add(itm);
                lstDevices.ItemsSource = _usbDevices;
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            txbStatus.Text = "";
            if (btn == btnConnect)
            {
                if (lstDevices.SelectedIndex == -1)
                    return;
                var baud = uint.Parse(((ComboBoxItem)cmbBands.SelectedItem).Content.ToString());
                if (radUSB.IsChecked.Value)
                {
                    var usbId = ((DeviceInformation)lstDevices.SelectedItem).Id.Split(new string[] { "VID_" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('&')[0];
                    MainPage.usb = new UsbSerial(usbId);
                    MainPage.usb.ConnectionEstablished += delegate
                    {
                        MainPage.isConnected = btnBasicControl.IsEnabled = btnIDE.IsEnabled = true;
                        MainPage.Arduino = new Microsoft.Maker.RemoteWiring.RemoteDevice(MainPage.usb);
                    };
                    MainPage.usb.ConnectionFailed += delegate { ViewMessage("Couldn't reach the selected device!", Publics.MessageType.Error); MainPage.isConnected = false; };
                    MainPage.usb.ConnectionLost += async (msg) => 
                    {
                        MainPage.isConnected = false;
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Frame.Navigate(typeof(MainView), "Connected Lost!\n" + msg); });
                    };
                    MainPage.usb.begin(baud, SerialConfig.SERIAL_8N1);
                }
                else
                {
                    var bluetoothName = ((DeviceInformation)lstDevices.SelectedItem).Name;
                    MainPage.bluetooth = new BluetoothSerial(bluetoothName);
                    MainPage.bluetooth.ConnectionEstablished += delegate 
                    {
                        MainPage.isConnected = btnBasicControl.IsEnabled = btnIDE.IsEnabled = true;
                        MainPage.Arduino = new Microsoft.Maker.RemoteWiring.RemoteDevice(MainPage.bluetooth);
                    };
                    MainPage.bluetooth.ConnectionFailed += delegate  { ViewMessage("Couldn't reach the selected device!", Publics.MessageType.Error); MainPage.isConnected = false; };
                    MainPage.bluetooth.ConnectionLost += async (msg) =>
                    {
                        MainPage.isConnected = false;
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Frame.Navigate(typeof(MainView), "Connected Lost!\n" + msg); });
                    };
                    MainPage.bluetooth.begin(baud, SerialConfig.SERIAL_8N1);
                }
                
            }
            else if (btn == btnRefresh)
            {
                if (radUSB.IsChecked.Value) rad_Checked(radUSB, null);
                else rad_Checked(radBluetooth, null);
            }
            else if (btn == btnBasicControl)
                Frame.Navigate(typeof(BasicControlView));
            else if (btn == btnIDE)
                Frame.Navigate(typeof(IDEView));
        }

        private void ViewMessage(string text, Publics.MessageType type)
        {
            txbStatus.Text = text;
            txbStatus.Foreground = (type == Publics.MessageType.Error) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Blue);
        }
    }
}
