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
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Project_Phoenix.Views
{
    public sealed partial class CGeneratingView : Page
    { 
        public CGeneratingView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            bool hasSerial = false;
            string result = "";
            result += "/*This code had been generated using Project Phoenix by MrMHK*/\n";
            result += "void setup() \n{\n}\n\n";
            result += "void loop() \n{\n";
            var text = await readStringFromLocalFile("temp.txt");
            var commands = text.Split('\n');
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i].StartsWith("Apply"))
                {
                    var parameter = commands[i].Substring(6, 2);
                    var pin = commands[i].Substring(16);
                    if (parameter.StartsWith("0"))
                        result += "digitalWrite(" + pin + ", LOW);\n";
                    else
                        result += "digitalWrite(" + pin + ", HIGH);\n";
                }
                else if (commands[i].StartsWith("Wait"))
                {
                    result += "delay(" + commands[i].Substring(5).Split(' ')[0] + ");\n";
                }
                else if (commands[i].StartsWith("Read"))
                {
                    var pin = commands[i].Substring(14);
                    result += "serial.println(digitalRead(" + pin + "));\n";
                    if (!hasSerial)
                    {
                        var r = result.Split(new string[] { "void setup() \n{" }, StringSplitOptions.None);
                        result = r[0] + "void setup () \n{\nserial.Begin(9600);" + r[1];
                        hasSerial = true;
                    }
                }
                else if (commands[i].StartsWith("Measure"))
                {
                    var pin = commands[i].Split(new string[] { "Pin " }, StringSplitOptions.RemoveEmptyEntries)[1];
                    result += "serial.println(analogRead(" + pin + "));\n";
                    if (!hasSerial)
                    {
                        var r = result.Split(new string[] { "void setup() \n{" }, StringSplitOptions.None);
                        result = r[0] + "void setup () \n{\nserial.Begin(9600);" + r[1];
                        hasSerial = true;
                    }
                }
                else if (commands[i].StartsWith("Send"))
                {
                    var value = commands[i].Substring(5, 2);
                    if (!(value[1] >= 48 && value[1] <= 57))
                        value = commands[i].Substring(5, 1);
                    var pin = byte.Parse(commands[i].Split(new string[] { "Pin " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    result += "analogWrite(" + pin + ", " + value + ");\n";
                }
                else if (commands[i].StartsWith("Increase voltage"))
                {
                    string _pin = commands[i].Split(new string[] { "pin " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                    Split(new string[] { " by" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string _incr = commands[i].Split(new string[] { "by " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                    Split(new string[] { "/255" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string _time = commands[i].Split(new string[] { "every " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                    Split(new string[] { " milli-second" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    result += "for (int i" + i.ToString() + " = 0; i" + i.ToString() + " <= 255; i" + i.ToString() + " += " + _incr + ")\n{\n";
                    result += "\tanalogWrite(" + _pin + ", i" + i.ToString() + ");\n";
                    result += "\tdelay(" + _time + ");\n}\n";
                }
                else if (commands[i].StartsWith("Decrease voltage"))
                {
                    string _pin = commands[i].Split(new string[] { "pin " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                    Split(new string[] { " by" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string _decr = commands[i].Split(new string[] { "by " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                    Split(new string[] { "/255" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string _time = commands[i].Split(new string[] { "every " }, StringSplitOptions.RemoveEmptyEntries)[1].
                                    Split(new string[] { " milli-second" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    result += "for (int i" + i.ToString() + " = 255; i" + i.ToString() + " >= 0; i" + i.ToString() + " -= " + _decr + ")\n{\n";
                    result += "\tanalogWrite(" + _pin + ", i" + i.ToString() + ");\n";
                    result += "\tdelay(" + _time + ");\n}\n";
                }
            }
            result += "}\n";
            txbCCode.Text = result;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Arduino Code File", new List<string>() { ".ino" });
            savePicker.SuggestedFileName = "CodeByProjectPhoenix";
            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var c = txbCCode.Text.ToArray();
                List<byte> buffer = new List<byte>();
                for (int i = 0; i < c.Length; i++)
                    buffer.Add(Convert.ToByte(c[i]));
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBytesAsync(file, buffer.ToArray());
                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                MessageDialog md = new MessageDialog(status.ToString());
                await md.ShowAsync();
            }
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
