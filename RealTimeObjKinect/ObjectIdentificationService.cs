using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace RealTimeObjKinect
{
    public class ObjectIdentificationService
    {

        private const float AgreementCutoffPercent = 0.8f;

        public static IList<string> AnalyzeImage(WriteableBitmap imageToAnalyze)
        {
            IList<string> recognizedObjects = new List<string>();

            //get the color palletes
            Dictionary<Color, ColorInformation> imagePalette = PaletteAnalyzer.GetColorPallete(imageToAnalyze);

            //see what it matches with
            List<ObjectSignatureData> signatures = ObjectMemoryService.GetSignatures();
            foreach (ObjectSignatureData signature in signatures)
            {
                float colors = signature.ObjectColors.Count;
                float matches = 0.0f;

                foreach (ObjectColorData colorData in signature.ObjectColors)
                {
                    if (imagePalette.ContainsKey(colorData.ObjectColor))
                    {
                        matches++;
                    }
                }

                if ((matches / colors) > AgreementCutoffPercent)
                {
                    recognizedObjects.Add(signature.ObjectName);
                }

            }

            return recognizedObjects;
        }

    }
}
