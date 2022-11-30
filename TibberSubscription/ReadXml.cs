using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace TibberSubscription
{
    /// <summary>
    /// Variables that should be defined in Resources.xml
    /// Resources.xml is mapped directly to corresponding variable in this class
    /// </summary>
    public class TibberResource
    {
        public string reconnect;
        public string productheader;
        public string productversion;
        public string apikey;
        public string brokeraddress;
        public int brokerport;
        public string basetopic;
        public string homeid;
        public int delay;
        public List<Topic> topics;
    }
    /// <summary>
    /// All topics defined in Resources.xml is mapped to <c>topics</c> list in <c>TibberResource</c> using this class
    /// </summary>
    [XmlRoot(ElementName = "Topic")]
    public class Topic
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "enabled")]
        public string Enabled { get; set; }
    }
    /// <summary>
    /// Read Resources.xml and map xml elements to <c>TibberResource</c> and <c>Topic</c> classes
    /// </summary>
    internal class ReadXml
    {
        public static TibberResource ReadXML()
        {
            // Resources.xml must be present in current directory.
            string paths = Directory.GetCurrentDirectory() + "\\Resources.xml";
            // Setup XML reader, and defined root attribute, in our case "resources"
            // and map elements to TibberResource class
            XmlSerializer reader = new XmlSerializer(typeof(TibberResource), new XmlRootAttribute("resources"));
            StreamReader file = new StreamReader(paths);
            TibberResource overview = (TibberResource)reader.Deserialize(file);
            file.Close();
            // Return TibberResource object containing all XML info needed
            return overview;
        }
    }
}
