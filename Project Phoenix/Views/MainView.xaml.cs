using Microsoft.Maker.Serial;
using Project_Phoenix.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            if (btn == btnConnect)
            {
                if (lstDevices.SelectedIndex == -1)
                {
                    return;
                }
                Device.UseUSB = radUSB.IsChecked.Value;
                if (Device.UseUSB)
                {
                    var usbId = ((DeviceInformation)lstDevices.SelectedItem).Id.Split(new string[] { "VID_" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('&')[0];
                    Device.Initiate(usbId, int.Parse(((ComboBoxItem)cmbBands.SelectedItem).Content.ToString()));
                }
                else Device.Initiate(((DeviceInformation)lstDevices.SelectedItem).Name, int.Parse(((ComboBoxItem)cmbBands.SelectedItem).Content.ToString()));
                btnBasicControl.IsEnabled = btnIDE.IsEnabled = true;
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
    }
}
