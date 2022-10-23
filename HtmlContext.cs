using System;
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
            public string IndexHtml { get; set; }

            public string ObjectHtml { get; set; }
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
        
        public string NugetStore { get; private set; } = "packages\\";

        public TemplateConfig Template { get; } = new TemplateConfig();

        public List<string> Definitions { get; } = new List<string>();

        public List<string> Scripts { get; } = new List<string>();

        public Dictionary<string, DocAssembly> Assemblies { get; } = new Dictionary<string, DocAssembly>();

        public List<NugetDefinition> Packages { get; } = new List<NugetDefinition>();

        /// <summary>
        /// Icon path stored by key name. The key name is taken from the XML tag name which defined the icon path in config.xml.
        /// </summary>
        public Dictionary<string, string> Icons { get; } = new Dictionary<string, string>();

        public Dictionary<string, DocNamespace> Namespaces { get; } = new Dictionary<string, DocNamespace>();

        public Dictionary<string, DocObject> ObjectsByQualifiedName { get; } = new Dictionary<string, DocObject>();

        public IndexConfig Index { get; } = new IndexConfig();

        public SummaryConfig Summary { get; } = new SummaryConfig();

        public static HtmlContext Load(string path)
        {
            HtmlContext cxt = new HtmlContext();
            XmlDocument doc = new XmlDocument();

            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    doc.Load(stream);

                XmlNode cfg = doc["config"];
                if (cfg == null)
                {
                    cxt.Log($"The root 'config' node was not found");
                    return cxt;
                }

                XmlNode dest = cfg["destination"];
                XmlNode defs = cfg["definitions"];
                XmlNode nuget = cfg["nuget"];
                XmlNode template = cfg["template"];
                XmlNode scripts = cfg["scripts"];
                XmlNode summary = cfg["summary"];
                XmlNode icons = cfg["icons"];
                XmlNode intro = cfg["intro"];


                if (dest != null)
                    cxt.DestinationPath = dest.InnerText;

                if (template != null)
                {
                    XmlNode xIndex = template["index"];
                    cxt.Template.IndexHtml = cxt.LoadTemplate(xIndex);

                    XmlNode xObject = template["object"];
                    cxt.Template.ObjectHtml = cxt.LoadTemplate(xObject);
                }

                foreach (XmlNode child in defs.ChildNodes)
                    cxt.Definitions.Add(child.InnerText);

                if (nuget != null)
                {
                    foreach (XmlNode nug in nuget.ChildNodes)
                    {
                        switch (nug.Name)
                        {
                            case "store":
                                cxt.NugetStore = nug.InnerText;
                                break;

                            case "package":
                                XmlNode xName = nug["name"];
                                XmlNode xVersion = nug["version"];
                                XmlNode xFramework = nug["framework"];

                                cxt.Packages.Add(new NugetDefinition()
                                {
                                    Name = xName != null ? xName.InnerText : "",
                                    Version = xVersion != null ? xVersion.InnerText : "",
                                    Framework = xFramework != null ? xFramework.InnerText : ""
                                });
                                break;
                        }
                    }
                }

                if (intro != null)
                    cxt.Index.Intro = intro.InnerText;

                // Summary config                
                if (summary != null)
                {
                    XmlNode sumMaxLength = summary["maxlength"];
                    if (sumMaxLength != null)
                    {
                        if (int.TryParse(sumMaxLength.InnerText, out int maxLen))
                            cxt.Summary.MaxLength = maxLen;
                    }

                    XmlNode sumReadMore = summary["readmore"];
                    if (sumReadMore != null)
                        cxt.Summary.ReadMore = sumReadMore.InnerText;
                }

                // Icon config
                if (icons != null)
                {
                    foreach (XmlNode iNode in icons.ChildNodes)
                        cxt.Icons.Add(iNode.Name.ToLower(), iNode.InnerText);
                }

                if (scripts != null)
                {
                    foreach (XmlNode iNode in scripts.ChildNodes)
                        cxt.Scripts.Add(iNode.InnerText);
                }
            }

            return cxt;
        }

        private string LoadTemplate(XmlNode pathNode)
        {
            if (pathNode != null)
            {
                string objPath = $"{DestinationPath}{pathNode.InnerText}";
                if (File.Exists(objPath))
                {
                    using (FileStream stream = new FileStream(objPath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                            return reader.ReadToEnd();
                    }
                }
                else
                {
                    Log($"Template not found: {objPath}");
                }
            }

            return null;
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(string message)
        {
            ConsoleColor pCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {message}");
            Console.ForegroundColor = pCol;
        }

        public string GetIcon(MemberInfo info, string pathPrefix = "")
        {
            return GetIcon(info.MemberType.ToString(), pathPrefix);
        }

        public string GetIcon(DocObject obj, string pathPrefix = "")
        {
            string iconName = obj.DocType.ToString().ToLower();
            return GetIcon(iconName, pathPrefix);
        }

        public string GetIcon(string iconName, string pathPrefix = "")
        {
            string html = "&nbsp;";

            if (!string.IsNullOrWhiteSpace(iconName))
            {
                if (Icons.TryGetValue(iconName.ToLower(), out string iconPath))
                    html = $"<img src=\"{pathPrefix}{iconPath}\" title=\"{iconName}\" alt\"{iconName} icon\"/>";
            }

            return html;
        }

        /// <summary>
        /// Converts a string into a safe file name.
        /// </summary>
        /// <param name="text">The text to be converted into a file-name.</param>
        /// <returns></returns>
        public string GetFileName(string text)
        {
            return text.Replace('<', '_').Replace('>', '_').Replace('.', '_'); 
        }

        public DocObject CreateObject(Type type)
        {
            string ns = type.Namespace;
            string qualifiedName = $"{ns}.{type.Name}";

            if (!ObjectsByQualifiedName.TryGetValue(qualifiedName, out DocObject obj))
            {
                obj = new DocObject(type.Name);
                obj.UnderlyingType = type;
                ObjectsByQualifiedName[qualifiedName] = obj;

                if (!Namespaces.TryGetValue(ns, out DocNamespace dnSpace))
                {
                    dnSpace = new DocNamespace(ns);
                    Namespaces.Add(ns, dnSpace);
                }

                dnSpace.Objects.Add(obj);
            }

            return obj;
        }

        public DocObject GetObject(string ns, string objName)
        {
            string qualifiedName = $"{ns}.{objName}";
            ObjectsByQualifiedName.TryGetValue(qualifiedName, out DocObject obj);
            return obj;
        }
    }
}
