using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Phoenix.Code
{
    public class Publics
    {
        public static List<string> lstDigitalPins = new List<string>()
        { "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13" };
        public static List<string> lstPWMPins = new List<string>()
        { "3", "5", "6", "9", "10", "11", };
        public static List<string> lstAnalogPins = new List<string>()
        { "A0", "A1", "A2", "A3", "A4", "A5",};
        
        public enum MessageType { Info, Error };
    }
}
