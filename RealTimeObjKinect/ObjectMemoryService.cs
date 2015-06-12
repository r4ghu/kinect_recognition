using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace RealTimeObjKinect
{
    public class ObjectMemoryService
    {

        private static List<ObjectSignatureData> objectSignatures = new List<ObjectSignatureData>();

        static ObjectMemoryService()
        {
            FileInfo fi = new FileInfo("..\\..\\objectlibrary\\objectsignatures.xml");
            if (fi.Exists)
            {
                using (FileStream reader = fi.OpenRead())
                {
                    try
                    {
                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ObjectSignatureData>));
                        objectSignatures = (List<ObjectSignatureData>)serializer.Deserialize(reader);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public static List<ObjectSignatureData> GetSignatures()
        {
            return objectSignatures;
        }

        public static void AddSignature(ObjectSignatureData newSignature)
        {
            objectSignatures.Add(newSignature);
            sync();
        }

        public static void RemoveSignatureByName(string name)
        {
            ObjectSignatureData objectToRemove = null;

            foreach (ObjectSignatureData objectSignature in objectSignatures)
            {
                if (objectSignature.ObjectName.Equals(name))
                {
                    objectToRemove = objectSignature;
                    break;
                }
            }

            if (objectToRemove != null)
            {
                objectSignatures.Remove(objectToRemove);
                sync();
            }
        }

        public static void sync()
        {
            InitializeObjectLibrary();
            FileInfo fi = new FileInfo("..\\..\\objectlibrary\\objectsignatures.xml");

            using (StreamWriter writer = fi.CreateText())
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ObjectSignatureData>));
                    serializer.Serialize(writer, objectSignatures);
                }
                catch (Exception ex)
                {
                }
            }
        }

        //ensures that object libary exists at the specified location
        private static void InitializeObjectLibrary()
        {
            if (!Directory.Exists("..\\..\\objectlibrary"))
            {
                Directory.CreateDirectory("..\\..\\objectlibrary");
            }
            //this is a work around for the XmlSerializer throwing an exception in some cases where it is 
            //writing to a file newly created with fi.createText
            FileInfo fi = new FileInfo("..\\..\\objectlibrary\\objectsignatures.xml");
            if (!fi.Exists)
            {
                using (StreamWriter writer = fi.CreateText())
                {
                    writer.Write(" ");
                }
            }
        }
    }
}
