using Project_Phoenix.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Project_Phoenix.Views
{
    public sealed partial class ExcutingView : Page
    {
        private CancellationTokenSource cts;
        public ExcutingView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Begin();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Stop();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton btn = (AppBarButton)sender;
            if (btn == btnPause)
            {
                if (btn.Label == "Pause")
                {
                    btn.Label = "Continue";
                    btn.Icon = new SymbolIcon(Symbol.Play);
                    txb.Text = "Stopped, hit Continue to start";
                    Stop();
                }
                else
                {
                    btn.Label = "Pause";
                    btn.Icon = new SymbolIcon(Symbol.Pause);
                    txb.Text = "Working, hit Pasue to stop";
                    Begin();
                }
            }
            else Frame.Navigate(typeof(CGeneratingView)); 
        }

        public void Begin()
        {
            cts = new CancellationTokenSource();
            Task t = Task.Factory.StartNew(async () =>
            {
                var text = await readStringFromLocalFile("temp.txt");
                var commands = text.Split('\n');
                while (true)
                {
                    for (int i = 0; i < commands.Length; i++)
                    {
                        if (commands[i].StartsWith("Applying"))
                        {
                            var parameter = commands[i].Substring(9, 2);
                            var pin = commands[i].Substring(19);
                            if (parameter.StartsWith("0"))
                                Device.Arduino.digitalWrite(byte.Parse(pin), Microsoft.Maker.RemoteWiring.PinState.LOW);
                            else
                                Device.Arduino.digitalWrite(byte.Parse(pin), Microsoft.Maker.RemoteWiring.PinState.HIGH);
                        }
                        else if (commands[i].StartsWith("Waiting"))
                        {
                            await Task.Delay(int.Parse(commands[i].Substring(8).Split(' ')[0]));
                        }
                        else if (commands[i].StartsWith("Reading"))
                        {
                            var pin = commands[i].Substring(17);
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
                                {
                                    TextBlock txb = new TextBlock() { Text = "Reading from pin " + pin + " is: " + Device.Arduino.digitalRead(byte.Parse(pin)).ToString() };
                                    txb.Margin = new Thickness(0, 10, 0, 0);
                                    stkOutputs.Children.Add(txb);
                                    srv.UpdateLayout();
                                    srv.ChangeView(0, stkOutputs.ActualHeight, 1.0f, false);
                                });
                        }
                        else if (commands[i].StartsWith("Measuring"))
                        {
                            var pin = commands[i].Split(new string[] { "Pin " }, StringSplitOptions.RemoveEmptyEntries)[1];
                            /*if (pin.Length == 1) _pin = byte.Parse(pin);
                            else _pin = byte.Parse((int.Parse(pin[1].ToString()) + 14).ToString());*/
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                TextBlock txb = new TextBlock() { Text = "Measuring pin " + pin + " is: " + Device.Arduino.analogRead(pin).ToString() };
                                txb.Margin = new Thickness(0, 10, 0, 0);
                                stkOutputs.Children.Add(txb);
                                srv.UpdateLayout();
                                srv.ChangeView(0, stkOutputs.ActualHeight, 1.0f, false);
                            });
                        }
                        else if (commands[i].StartsWith("Sending"))
                        {
                            var value = commands[i].Substring(8, 2);
                            if (!(value[1] >= 48 && value[1] <= 57))
                                value = commands[i].Substring(8, 1);
                            var pin = byte.Parse(commands[i].Split(new string[] { "Pin " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                            Device.Arduino.analogWrite(pin, (ushort)int.Parse(value));
                        }
                        else if (commands[i].StartsWith("Increasing voltage"))
                        {
                            string _pin = commands[i].Split(new string[] { "pin " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                            Split(new string[] { " by" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            string _incr = commands[i].Split(new string[] { "by " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                            Split(new string[] { "/255" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            string _time = commands[i].Split(new string[] { "every " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                            Split(new string[] { " milli-second" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            byte pin = byte.Parse(_pin);
                            int incr = int.Parse(_incr), time = int.Parse(_time);
                            for (int j = 0; j <= 255; j += incr)
                            {
                                Device.Arduino.analogWrite(pin, (ushort)j);
                                await Task.Delay(time);
                            }
                        }
                        else if (commands[i].StartsWith("Decreasing voltage"))
                        {
                            string _pin = commands[i].Split(new string[] { "pin " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                            Split(new string[] { " by" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            string _decr = commands[i].Split(new string[] { "by " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                            Split(new string[] { "/255" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            string _time = commands[i].Split(new string[] { "every " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                            Split(new string[] { " milli-second" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            byte pin = byte.Parse(_pin);
                            int decr = int.Parse(_decr), time = int.Parse(_time);
                            for (int j = 255; j >= 0; j -= decr)
                            {
                                Device.Arduino.analogWrite(pin, (ushort)j);
                                await Task.Delay(time);
                            }
                        }
                        else continue;
                        if (cts.IsCancellationRequested) break;
                    }
                    if (cts.IsCancellationRequested) break;
                }
            }, cts.Token);
        }

        public void Stop()
        {
            cts.Cancel();
        }

        private static async Task<string> readStringFromLocalFile(string filename)
        {
            StorageFolder local = ApplicationData.Current.LocalFolder;
            Stream stream = await local.OpenStreamForReadAsync(filename);
            string text;
            using (StreamReader reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }
            return text;
        }
        
    }
}
