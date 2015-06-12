

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace RealTimeObjKinect
{
    /// <summary>
    /// This class provides a utility for persisting System.Drawing.Color 
    /// to an from a string for use writing to or reading from XML
    /// </summary>
    public class ColorConverter
    {

        public static String ToXmlString(Color color)
        {
            string colorString = "";

            colorString += GetThreeDigitString(color.A);
            colorString += GetThreeDigitString(color.B);
            colorString += GetThreeDigitString(color.G);
            colorString += GetThreeDigitString(color.R);

            return colorString;
        }

        public static Color FromXmlString(string colorString)
        {
            //this will blow up if it gets the wrong size string
            byte alpha = (byte)int.Parse(colorString.Substring(0, 3));
            byte blue = (byte)int.Parse(colorString.Substring(3, 3));
            byte green = (byte)int.Parse(colorString.Substring(6, 3));
            byte red = (byte)int.Parse(colorString.Substring(9, 3));

            return Color.FromArgb(alpha, red, green, blue);
        }

        static string GetThreeDigitString(int value)
        {
            string returnValue = "";
            if (value < 10)
            {
                returnValue = "00";
                returnValue += value;
            }
            else if (value < 100)
            {
                returnValue = "0";
                returnValue += value;
            }
            else
            {
                returnValue += value;
            }

            return returnValue;
        }

    }
}
