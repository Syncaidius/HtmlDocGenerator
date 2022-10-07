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
        public class IndexConfig
        {
            public string Intro { get; set; }
        }

        public class SummaryConfig
        {
            public int MaxLength { get; set; } = 300;

            /// <summary>
            /// Gets or sets the 'read more' text.
            /// </summary>
            public string ReadMore { get; set; } = "Read More";
        }

        public string Template { get; protected set; }

        public List<string> Definitions { get; } = new List<string>();

        public List<NugetDefinition> Packages { get; } = new List<NugetDefinition>();

        public IndexConfig Index { get; } = new IndexConfig();

        public SummaryConfig Summary { get; } = new SummaryConfig();

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

                // Index config
                XmlNode index = doc["config"]["index"];
                if(index != null)
                {
                    XmlNode intro = index["intro"];
                    if (intro != null)
                        config.Index.Intro = intro.InnerText;
                }

                // Summary config
                XmlNode summary = doc["config"]["summary"];
                if(summary != null)
                {
                    XmlNode sumMaxLength = summary["maxlength"];
                    if(sumMaxLength != null)
                    {
                        if (int.TryParse(sumMaxLength.InnerText, out int maxLen))
                            config.Summary.MaxLength = maxLen;
                    }

                    XmlNode sumReadMore = summary["readmore"];
                    if(sumReadMore != null)
                        config.Summary.ReadMore = sumReadMore.InnerText;
                }
            }

            return config;
        }
    }
}
