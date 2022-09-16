using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HtmlDocGenerator
{
    public class DocParser
    {
        public DocParser()
        {

        }

        public Documentation Parse(FileStream stream)
        {
            string json = null;
            using (XmlReader reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();
                //reader.ReadToDescendant("doc");
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);
                json = JsonConvert.SerializeXmlNode(xml, Newtonsoft.Json.Formatting.Indented, true);
            }

            return JsonConvert.DeserializeObject<Documentation>(json);
        }
    }
}
