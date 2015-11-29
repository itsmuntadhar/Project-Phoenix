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
                if (commands[i].StartsWith("Applying"))
                {
                    var parameter = commands[i].Substring(9, 2);
                    var pin = commands[i].Substring(19);
                    if (parameter.StartsWith("0"))
                        result += "digitalWrite(" + pin + ", LOW);\n";
                    else
                        result += "digitalWrite(" + pin + ", HIGH);\n";
                }
                else if (commands[i].StartsWith("Waiting"))
                {
                    result += "delay(" + commands[i].Substring(8).Split(' ')[0] + ");\n";
                }
                else if (commands[i].StartsWith("Reading"))
                {
                    var pin = commands[i].Substring(17);
                    result += "serial.println(digitalRead(" + pin + "));\n";
                    if (!hasSerial)
                    {
                        var r = result.Split(new string[] { "void setup() \n{" }, StringSplitOptions.None);
                        result = r[0] + "void setup () \n{\nserial.Begin(9600);" + r[1];
                        hasSerial = true;
                    }
                }
                else if (commands[i].StartsWith("Measuring"))
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
                else if (commands[i].StartsWith("Sending"))
                {
                    var value = commands[i].Substring(8, 2);
                    if (!(value[1] >= 48 && value[1] <= 57))
                        value = commands[i].Substring(8, 1);
                    var pin = byte.Parse(commands[i].Split(new string[] { "Pin " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    result += "analogWrite(" + pin + ", " + value + ");\n";
                }
            }
            result += "}\n";
            txbCCode.Text = result;
            /*Task t = Task.Factory.StartNew(async () =>
            {
                
            }, cts.Token);*/
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
