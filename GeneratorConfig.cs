using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace HtmlDocGenerator
{
    public class GeneratorConfig
    {
        public string Template { get; protected set; }
        public List<string> Definitions { get; }

        public List<NugetDefinition> Packages {get;}

        public GeneratorConfig()
        {
            Definitions = new List<string>();
            Packages = new List<NugetDefinition>();
        }

        public static GeneratorConfig Load(string path)
        {
            GeneratorConfig config = new GeneratorConfig();
            XmlDocument doc = new XmlDocument();

            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    doc.Load(stream);

                XmlNode defs = doc["config"]["definitions"];
                XmlNode nugets = doc["config"]["nuget"];
                XmlNode template = doc["config"]["template"];

                config.Template = template.InnerText;

                foreach (XmlNode child in defs.ChildNodes)
                    config.Definitions.Add(child.InnerText);

                foreach (XmlNode nug in nugets.ChildNodes)
                {
                    XmlNode xName = nug["name"];
                    XmlNode xVersion = nug["version"];
                    XmlNode xFramework = nug["framework"];

                    config.Packages.Add(new NugetDefinition()
                    {
                        Name = xName != null ? xName.InnerText : "",
                        Version = xVersion != null ? xVersion.InnerText : "",
                        Framework = xFramework != null ? xFramework.InnerText : ""
                    });
                }
            }

            return config;
        }
    }
}
