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
                        if (commands[i].StartsWith("Apply"))
                        {
                            var parameter = commands[i].Substring(6, 2);
                            var pin = commands[i].Substring(16);
                            if (parameter.StartsWith("0"))
                                MainPage.Arduino.digitalWrite(byte.Parse(pin), Microsoft.Maker.RemoteWiring.PinState.LOW);
                            else
                                MainPage.Arduino.digitalWrite(byte.Parse(pin), Microsoft.Maker.RemoteWiring.PinState.HIGH);
                        }
                        else if (commands[i].StartsWith("Wait"))
                        {
                            await Task.Delay(int.Parse(commands[i].Substring(5).Split(' ')[0]));
                        }
                        else if (commands[i].StartsWith("Read"))
                        {
                            var pin = commands[i].Substring(14);
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
                                {
                                    TextBlock txb = new TextBlock() { Text = "Reading from pin " + pin + " is: " + MainPage.Arduino.digitalRead(byte.Parse(pin)).ToString() };
                                    txb.Margin = new Thickness(0, 10, 0, 0);
                                    stkOutputs.Children.Add(txb);
                                    srv.UpdateLayout();
                                    srv.ChangeView(0, stkOutputs.ActualHeight, 1.0f, false);
                                });
                        }
                        else if (commands[i].StartsWith("Measure"))
                        {
                            var pin = commands[i].Split(new string[] { "Pin " }, StringSplitOptions.RemoveEmptyEntries)[1];
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                TextBlock txb = new TextBlock() { Text = "Measuring pin " + pin + " is: " + MainPage.Arduino.analogRead(pin).ToString() };
                                txb.Margin = new Thickness(0, 10, 0, 0);
                                stkOutputs.Children.Add(txb);
                                srv.UpdateLayout();
                                srv.ChangeView(0, stkOutputs.ActualHeight, 1.0f, false);
                            });
                        }
                        else if (commands[i].StartsWith("Send"))
                        {
                            var value = commands[i].Substring(5, 2);
                            if (!(value[1] >= 48 && value[1] <= 57))
                                value = commands[i].Substring(5, 1);
                            var pin = byte.Parse(commands[i].Split(new string[] { "Pin " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                            MainPage.Arduino.analogWrite(pin, (ushort)int.Parse(value));
                        }
                        else if (commands[i].StartsWith("Increase voltage"))
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
                                MainPage.Arduino.analogWrite(pin, (ushort)j);
                                await Task.Delay(time);
                            }
                        }
                        else if (commands[i].StartsWith("Decrease voltage"))
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
                                MainPage.Arduino.analogWrite(pin, (ushort)j);
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
