using Project_Phoenix.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class BasicControlView : Page
    {
        public BasicControlView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            sldAnalogPin3.ValueChanged += Sld_ValueChanged;
            sldAnalogPin5.ValueChanged += Sld_ValueChanged;
            sldAnalogPin6.ValueChanged += Sld_ValueChanged;
            sldAnalogPin9.ValueChanged += Sld_ValueChanged;
            sldAnalogPin10.ValueChanged += Sld_ValueChanged;
            sldAnalogPin11.ValueChanged += Sld_ValueChanged;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            sldAnalogPin3.ValueChanged -= Sld_ValueChanged;
            sldAnalogPin5.ValueChanged -= Sld_ValueChanged;
            sldAnalogPin6.ValueChanged -= Sld_ValueChanged;
            sldAnalogPin9.ValueChanged -= Sld_ValueChanged;
            sldAnalogPin10.ValueChanged -= Sld_ValueChanged;
            sldAnalogPin11.ValueChanged -= Sld_ValueChanged;
        }

        private void btnDigitalPin_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            var pinNumber = btn.Name.Split(new string[] { "Pin" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
            if (!(pinNumber[1] >= 48 && pinNumber[1] <= 57)) pinNumber = pinNumber.Substring(0, 1);
            else pinNumber = pinNumber.Substring(0, 2);
            if (btn.Name.Contains("Write"))
            {
                if ((string)btn.Content == "Off")
                {
                    btn.Background = btn.BorderBrush = new SolidColorBrush(Colors.Green);
                    btn.Content = "On";
                    MainPage.Arduino.digitalWrite(byte.Parse(pinNumber), Microsoft.Maker.RemoteWiring.PinState.HIGH);
                }
                else
                {
                    btn.Background = btn.BorderBrush = new SolidColorBrush(Colors.Red);
                    btn.Content = "Off";
                    MainPage.Arduino.digitalWrite(byte.Parse(pinNumber), Microsoft.Maker.RemoteWiring.PinState.LOW);
                }
            }
            else if (btn.Name.Contains("Read")) 
            {
                var res = MainPage.Arduino.digitalRead(byte.Parse(pinNumber));
                var p = (StackPanel)btn.Parent;
                var b = (Button)p.Children[1];
                if (res == Microsoft.Maker.RemoteWiring.PinState.HIGH)
                {
                    b.Background = b.BorderBrush = new SolidColorBrush(Colors.Green);
                    b.Content = "On";
                }
                else
                {
                    b.Background = b.BorderBrush = new SolidColorBrush(Colors.Red);
                    b.Content = "Off";
                }
            }
        }
        
        private void btnAnalogPin_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Slider sld = (Slider)((StackPanel)((StackPanel)(btn.Parent)).Parent).Children[1];
            var pinNumber = btn.Name.Split(new string[] { "Pin" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
            if (!(pinNumber[1] >= 48 && pinNumber[1] <= 57)) pinNumber = pinNumber.Substring(0, 1);
            else pinNumber = pinNumber.Substring(0, 2);
            if ((string)btn.Content == "Read")
            {
                //sld.IsEnabled = false;
                sld.ValueChanged -= Sld_ValueChanged;
                sld.Value = MainPage.Arduino.analogRead(pinNumber);
                btn.Content = "Write";
            }
            else
            {
                //sld.IsEnabled = true;
                sld.ValueChanged += Sld_ValueChanged;
                btn.Content = "Read";
            }
        }

        private void Sld_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider sld = (Slider)sender;
            var pinNumber = sld.Name.Split(new string[] { "Pin" }, StringSplitOptions.RemoveEmptyEntries)[1];
            if (pinNumber.Length > 1)
            { 
                if (!(pinNumber[1] >= 48 && pinNumber[1] <= 57)) pinNumber = pinNumber.Substring(0, 1);
                else pinNumber = pinNumber.Substring(0, 2);
            }
            MainPage.Arduino.analogWrite(byte.Parse(pinNumber), (ushort)sld.Value);
        }
    }
}
