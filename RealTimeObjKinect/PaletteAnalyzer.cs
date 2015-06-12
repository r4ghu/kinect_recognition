using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;



namespace RealTimeObjKinect
{
    public class PaletteAnalyzer
    {
        private const int ColorBandWidth = 16;
        private Dictionary<Color, Color> withLearningObjectColors = new Dictionary<Color, Color>();
        private Dictionary<Color, Color> withoutLearningObjectColors = new Dictionary<Color, Color>();

        public static Dictionary<Color, ColorInformation> AnalyzeBitmaps(WriteableBitmap imageWithObject, WriteableBitmap imageWithoutObject)
        {
            Bitmap bitmapWithObject = BitmapImage2Bitmap(imageWithObject);
            Bitmap bitmapWithoutObject = BitmapImage2Bitmap(imageWithoutObject);

            //get the pallet for each, these are already normalize into color bands
            Dictionary<Color, ColorInformation> backgroundPallete = GetColorPallete(bitmapWithoutObject);
            Dictionary<Color, ColorInformation> objectPallete = GetColorPallete(bitmapWithObject);
            Dictionary<Color, ColorInformation> resultPallete = new Dictionary<Color, ColorInformation>();

            //if the color is absent from the background or 10 times more common in the learning
            //bitmp, then it is in the color pallete of the object to learn
            foreach (Color color in objectPallete.Keys)
            {
                ColorInformation backgroundColorInfo = null;
                ColorInformation objectColorInfo = null;
                backgroundPallete.TryGetValue(color, out backgroundColorInfo);
                objectPallete.TryGetValue(color, out objectColorInfo);

                if ((backgroundColorInfo == null) || ((backgroundColorInfo.numberOfPixels * 10) < objectColorInfo.numberOfPixels))
                {
                    resultPallete.Add(color, objectColorInfo);
                }
            }

            return resultPallete;
        }

        public static Dictionary<Color, ColorInformation> GetColorPallete(WriteableBitmap bmpImage)
        {
            Bitmap bitmap = BitmapImage2Bitmap(bmpImage);
            return PaletteAnalyzer.GetColorPallete(bitmap);
        }

        private static Dictionary<Color, ColorInformation> GetColorPallete(Bitmap bmp)
        {
            //this is 
            Dictionary<Color, ColorInformation> pallete = new Dictionary<Color, ColorInformation>();

            for (int column = 0; column < bmp.Width; column++)
            {
                for (int row = 0; row < bmp.Height; row++)
                {
                    Color pixelColor = bmp.GetPixel(column, row);

                    //this create a color that is in the middle of the color band
                    Color normalColor = NormalizeColor(pixelColor);
                    ColorInformation ci = null;

                    if (pallete.TryGetValue(normalColor, out ci))
                    {
                        ci.numberOfPixels++;
                    }
                    else
                    {
                        ci = new ColorInformation();
                        ci.numberOfPixels = 1;
                        ci.PalleteColor = normalColor;
                        pallete.Add(normalColor, ci);
                    }
                }
            }

            return pallete;
        }


        //snippet from http://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        protected static Bitmap BitmapImage2Bitmap(WriteableBitmap bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                // return bitmap; <-- leads to problems, stream is closed/closing ...
                return new Bitmap(bitmap);
            }
        }

        private static Color NormalizeColor(Color color)
        {
            Byte normalRed = (Byte)((((int)(color.R / ColorBandWidth)) * ColorBandWidth) + (ColorBandWidth / 2));
            Byte normalGreen = (Byte)((((int)(color.G / ColorBandWidth)) * ColorBandWidth) + (ColorBandWidth / 2));
            Byte normalBlue = (Byte)((((int)(color.B / ColorBandWidth)) * ColorBandWidth) + (ColorBandWidth / 2));

            return Color.FromArgb(normalRed, normalGreen, normalBlue);
        }

    }
}
