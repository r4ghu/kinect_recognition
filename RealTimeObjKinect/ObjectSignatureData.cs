using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeObjKinect
{
    [Serializable()]
    public class ObjectSignatureData
    {
        public ObjectSignatureData()
        {
        }

        public ObjectSignatureData(List<ObjectColorData> objectColorData, String name)
        {
            this.ObjectColors = objectColorData;
            this.ObjectName = name;
        }

        [System.Xml.Serialization.XmlArray("Colors")]
        public List<ObjectColorData> ObjectColors { get; set; }

        [System.Xml.Serialization.XmlElement("ObjectName")]
        public string ObjectName { get; set; }
    }
}
