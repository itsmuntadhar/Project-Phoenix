using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class IDEView : Page
    {
        public IDEView()
        {
            this.InitializeComponent();
            cmbCommands.SelectionChanged += cmbCommands_SelectionChanged;
        }

        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn == btnAdd)
            {
                #region AddingCommand
                txtValue.BorderBrush = new SolidColorBrush(Colors.Gray);
                string command = "";
                string cmd = (string)(((ComboBoxItem)cmbCommands.SelectedItem).Content);
                if (cmd == "Digital Write")
                {
                    string pin = cmbPins.SelectedItem.ToString();
                    if (txtValue.Text == "0")
                    {
                        command = "Applying 0v on Pin " + pin;
                    }
                    else if (txtValue.Text == "1")
                    {
                        command = "Applying 5v on Pin " + pin;
                    }
                    else
                    {
                        txtValue.BorderBrush = new SolidColorBrush(Colors.Red);
                        return;
                    }
                }
                else if (cmd == "Digital Read")
                {
                    string pin = cmbPins.SelectedItem.ToString();
                    command = "Reading from Pin " + pin;
                }
                else if (cmd == "Analog Write")
                {
                    string pin = cmbPins.SelectedItem.ToString();
                    int val;
                    if (int.TryParse(txtValue.Text, out val))
                    {
                        if (val >= 0 && val <= 255)
                            command = "Sending " + txtValue.Text + " to Pin " + pin;
                        else
                        {
                            txtValue.BorderBrush = new SolidColorBrush(Colors.Red);
                            return;
                        }
                    }
                    else
                    {
                        txtValue.BorderBrush = new SolidColorBrush(Colors.Red);
                        return;
                    }
                }
                else if (cmd == "Analog Read")
                {
                    string pin = cmbPins.SelectedItem.ToString();
                    command = "Measuring Pin " + pin;
                }
                else if (cmd == "Delay")
                {
                    int val;
                    if (int.TryParse(txtValue.Text, out val))
                    {
                        command = "Waiting " + txtValue.Text + " milliseconds";
                    }
                    else
                    {
                        txtValue.BorderBrush = new SolidColorBrush(Colors.Red);
                        return;
                    }
                }
                else if (cmd == "Constant Increment" || cmd == "Constant Decrement")
                {
                    grdAdditionalParameters.Visibility = Visibility.Visible;
                }
                if (command != "") lstCommands.Items.Add(command);
                #endregion
            }
            else if (btn == btnExcute)
            {
                string txt = "";
                for (int i = 0; i < lstCommands.Items.Count; i++)
                    txt += lstCommands.Items[i].ToString() + "\n";
                await saveStringToLocalFile("temp.txt", txt);
                Frame.Navigate(typeof(ExcutingView));
            }
            else if (btn == btnSubmit)
            {
                int pin, time, val;
                if (txtPin.Text == "" || !int.TryParse(txtPin.Text, out pin))
                {
                    txtPin.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
                else if (txtTime.Text == "" || !int.TryParse(txtTime.Text, out time))
                {
                    txtTime.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
                else if (txtIncr.Text == "" || !int.TryParse(txtIncr.Text, out val))
                {
                    txtIncr.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
                string cmd = (string)(((ComboBoxItem)cmbCommands.SelectedItem).Content);
                if (cmd == "Constant Increment") lstCommands.Items.Add(string.Format("Increasing voltage on pin {0} by {1}/255 every {2} milli-second", pin, val, time));
                else lstCommands.Items.Add(string.Format("Decreasing voltage on pin {0} by {1}/255 every {2} milli-second", pin, val, time));
                txtIncr.Text = txtPin.Text = txtTime.Text = "";
                grdAdditionalParameters.Visibility = Visibility.Collapsed;
            }
            else if (btn == btnCancel)
            {
                txtIncr.Text = txtPin.Text = txtTime.Text = "";
                grdAdditionalParameters.Visibility = Visibility.Collapsed;
            }
        }

        private void cmbCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cmd = (string)(((ComboBoxItem)cmbCommands.SelectedItem).Content);
            txtValue.Text = "";
            if (cmd == "Digital Write")
            {
                cmbPins.IsEnabled = true;
                cmbPins.ItemsSource = null;
                cmbPins.ItemsSource = Code.Publics.lstDigitalPins;
                txtValue.IsEnabled = true;
                txtValue.PlaceholderText = "1 or 0";
            }
            else if (cmd == "Digital Read")
            {
                cmbPins.IsEnabled = true;
                cmbPins.ItemsSource = null;
                cmbPins.ItemsSource = Code.Publics.lstDigitalPins;
                txtValue.IsEnabled = false;
            }
            else if (cmd == "Analog Write")
            {
                cmbPins.IsEnabled = true;
                cmbPins.ItemsSource = null;
                cmbPins.ItemsSource = Code.Publics.lstPWMPins;
                txtValue.IsEnabled = true;
                txtValue.PlaceholderText = "from 0 to 255";
            }
            else if (cmd == "Analog Read")
            {
                cmbPins.IsEnabled = true;
                cmbPins.ItemsSource = null;
                var lst = new List<string>();
                foreach (var p in Code.Publics.lstAnalogPins) lst.Add(p);
                foreach (var p in Code.Publics.lstPWMPins) lst.Add(p);
                cmbPins.ItemsSource = lst;
                txtValue.IsEnabled = false;
            }
            else if (cmd == "Delay")
            {
                cmbPins.IsEnabled = false;
                txtValue.IsEnabled = true;
                txtValue.PlaceholderText = "time in ms";
            }
            else if (cmd == "Constant Increment" || cmd == "Constant Decrement")
            {
                cmbPins.IsEnabled = false;
                txtValue.IsEnabled = false;
            }
        }

        private async Task saveStringToLocalFile(string filename, string content)
        {
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());
            try
            {
                var f = await ApplicationData.Current.LocalFolder.GetFileAsync("temp.txt");
                if (f != null) await f.DeleteAsync();
            }
            catch { }
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
                stream.Write(fileBytes, 0, fileBytes.Length);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            lstCommands.Items.Remove(lstCommands.SelectedItem);
        }
    }
}
