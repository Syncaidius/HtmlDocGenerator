using Newtonsoft.Json;
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
    [JsonObject(MemberSerialization.OptIn)]
    public class DocContext : DocElement
    {
        public DocContext(string name) : 
            base(name, DocObjectType.Namespace)
        {

        }

        [JsonProperty]
        public string Intro { get; set; }

        public class CssConfig
        {
            public string Target { get; set; } = "doc-target";

            public string Invalid { get; set; } = "doc-invalid";
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

        public List<string> Definitions { get; } = new List<string>();

        public List<string> Scripts { get; } = new List<string>();

        public Dictionary<string, DocAssembly> Assemblies { get; } = new Dictionary<string, DocAssembly>();

        public List<NugetDefinition> Packages { get; } = new List<NugetDefinition>();

        public Dictionary<string, DocObject> ObjectsByQualifiedName { get; } = new Dictionary<string, DocObject>();

        public CssConfig Css { get; } = new CssConfig()
        {
            Target = "doc-target",
            Invalid = "doc-invalid"
        };

        public DirectoryInfo SourceDirectory { get; set; }

        public override string Namespace { get; } = "";

        public static DocContext Load(string title, string path)
        {
            DocContext cxt = new DocContext(title);
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
                XmlNode scripts = cfg["scripts"];
                XmlNode summary = cfg["summary"];
                XmlNode icons = cfg["icons"];
                XmlNode intro = cfg["intro"];
                XmlNode source = cfg["source"];
                XmlNode css = cfg["css"];

                if (dest != null)
                    cxt.DestinationPath = dest.InnerText;

                if (source != null)
                {
                    string dir = source.InnerText;
                    if (!Path.IsPathFullyQualified(dir))
                        dir = Path.GetFullPath(dir);

                    cxt.SourceDirectory = new DirectoryInfo(dir);
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
                    cxt.Intro = intro.InnerText;

                // Css config
                if (css != null)
                {
                    XmlNode docTarget = css["target"];
                    XmlNode docInvalid = css["invalid"];

                    if (docTarget != null)
                        cxt.Css.Target = docTarget.InnerText;

                    if (docInvalid != null)
                        cxt.Css.Invalid = docInvalid.InnerText;
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
            if (!ObjectsByQualifiedName.TryGetValue(type.FullName, out DocObject obj))
            {
                string objName = type.Name;

                if (type.IsGenericType)
                    objName = HtmlHelper.GetHtmlName(type);

                obj = new DocObject(objName);
                obj.UnderlyingType = type;
                ObjectsByQualifiedName[type.FullName] = obj;

                string[] nsParts = type.Namespace.Split('.');
                DocElement parent = this;
                for(int i = 0; i < nsParts.Length; i++)
                {
                    if (!parent.Members.TryGetValue(nsParts[i], out List<DocElement> oList))
                    {
                        oList = new List<DocElement>();
                        parent.Members.Add(nsParts[i], oList);
                        parent = new DocNamespace(nsParts[i]);
                        oList.Add(parent);
                    }
                    else
                    {
                        parent = oList[0];
                    }
                }

                parent.AddMember(obj);
            }

            return obj;
        }

        public DocObject GetObject(string ns, string objName)
        {
            string qualifiedName = $"{ns}.{objName}";
            ObjectsByQualifiedName.TryGetValue(qualifiedName, out DocObject obj);
            return obj;
        }

        [JsonProperty]
        public override string Name
        {
            get => base.Name;
            set => base.Name = value;
        }
    }
}
