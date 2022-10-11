﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace HtmlDocGenerator
{
    public class HtmlContext
    {
        public class IndexConfig
        {
            public string Intro { get; set; }
        }

        public class TemplateConfig
        {
            public string Index { get; set; }

            public string Object { get; set; }
        }

        public class SummaryConfig
        {
            public int MaxLength { get; set; } = 300;

            /// <summary>
            /// Gets or sets the 'read more' text.
            /// </summary>
            public string ReadMore { get; set; } = "Read More";
        }

        public string DestinationPath { get; set; } = "docs\\";
        public TemplateConfig Template { get; } = new TemplateConfig();

        public List<string> Definitions { get; } = new List<string>();

        public List<string> Scripts { get; } = new List<string>();

        public List<NugetDefinition> Packages { get; } = new List<NugetDefinition>();

        /// <summary>
        /// Icon path stored by key name. The key name is taken from the XML tag name which defined the icon path in config.xml.
        /// </summary>
        public Dictionary<string, string> Icons { get; } = new Dictionary<string, string>();

        public IndexConfig Index { get; } = new IndexConfig();

        public SummaryConfig Summary { get; } = new SummaryConfig();

        public static HtmlContext Load(string path)
        {
            HtmlContext config = new HtmlContext();
            XmlDocument doc = new XmlDocument();

            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    doc.Load(stream);

                XmlNode dest = doc["config"]["destination"];
                XmlNode defs = doc["config"]["definitions"];
                XmlNode nugets = doc["config"]["nuget"];
                XmlNode template = doc["config"]["template"];
                XmlNode scripts = doc["config"]["scripts"];
                XmlNode index = doc["config"]["index"];
                XmlNode summary = doc["config"]["summary"];
                XmlNode icons = doc["config"]["icons"];

                if (dest != null)
                    config.DestinationPath = dest.InnerText;

                if (template != null)
                {
                    XmlNode xIndex = template["index"];
                    if (xIndex != null)
                        config.Template.Index = xIndex.InnerText;

                    XmlNode xObject = template["object"];
                    if (xObject != null)
                        config.Template.Object = xObject.InnerText;
                }

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
                
                if(index != null)
                {
                    XmlNode intro = index["intro"];
                    if (intro != null)
                        config.Index.Intro = intro.InnerText;
                }

                // Summary config
                
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

                // Icon config
                if(icons != null)
                {
                    foreach (XmlNode iNode in icons.ChildNodes)
                        config.Icons.Add(iNode.Name.ToLower(), iNode.InnerText);
                }

                if(scripts != null)
                {
                    foreach (XmlNode iNode in scripts.ChildNodes)
                        config.Scripts.Add(iNode.InnerText);
                }
            }

            return config;
        }

        public bool Validate()
        {
            if (!File.Exists(Template.Index))
            {
                Console.WriteLine($"Index page template not found: {Template.Index}");
                return false;
            }

            if (!File.Exists(Template.Object))
            {
                Console.WriteLine($"Object page template not found: {Template.Object}");
                return false;
            }

            return true;
        }

        public string GetIcon(MemberInfo info, string pathPrefix = "")
        {
            return GetIcon(info.MemberType.ToString(), pathPrefix);
        }

        public string GetIcon(DocObject obj, string pathPrefix = "")
        {
            string iconName = obj.SubType.ToString().ToLower();
            return GetIcon(iconName, pathPrefix);
        }

        public string GetIcon(string iconName, string pathPrefix = "")
        {
            string html = "&nbsp;";

            if (!string.IsNullOrWhiteSpace(iconName))
            {
                if (Icons.TryGetValue(iconName.ToLower(), out string iconPath))
                    html = $"<img src=\"{pathPrefix}{iconPath}\"/>";
            }

            return html;
        }
    }
}