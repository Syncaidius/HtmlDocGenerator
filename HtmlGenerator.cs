using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HtmlDocGenerator
{
    public class HtmlGenerator
    {
        string _templateHtml;
        GeneratorConfig _config;

        public HtmlGenerator(GeneratorConfig config)
        {
            _config = config;
            if (!File.Exists(_config.Template))
            {
                Console.WriteLine($"Template not found: {_config.Template}");
                return;
            }

            // Read template html file.
            using (FileStream stream = new FileStream(_config.Template, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    _templateHtml = reader.ReadToEnd();
                    IsTemplateValid = true;
                }
            }
        }

        /// <summary>
        /// Generates an index page listing all of the available namespaces in the documented assemblies.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="indexPath"></param>
        public void Generate(List<DocData> docs, string destPath, string indexPath)
        {
            Dictionary<string, List<DocObject>> namespaceList = new Dictionary<string, List<DocObject>>();
            foreach (DocData doc in docs)
            {
                foreach (DocObject obj in doc.Members.Values)
                    Translate(obj, "", namespaceList);
            }

            // Output namespaces
            string docHtml = $"<p>{_config.Index.Intro}</p>";
            List<string> nsList = namespaceList.Keys.ToList();
            nsList.Sort();

            foreach (string ns in nsList)
            {
                string nsEscaped = ns.Replace('.', '_');
                List<DocObject> objList = namespaceList[ns];

                docHtml += $"<div id=\"{nsEscaped}\" class=\"sec-namespace\">";
                docHtml += $"<span class=\"namespace-toggle\">{ns}</span><br/>"; //<a href=\"docs\\{nsEscaped}.html\">{ns}</a>
                docHtml += "    <div class=\"sec-namespace-inner\">";

                foreach (DocObject obj in objList)
                {
                    docHtml += $"   <div id=\"{nsEscaped}-{obj.HtmlName}\" class=\"sec-namespace-obj\">";
                    docHtml += $"       {obj.HtmlName}<br/>{Environment.NewLine}";
                    docHtml += "    </div>";
                }

                docHtml += "</div></div></br>";

                
                GenerateObjectIndexPage(ns, indexPath, objList, destPath);
            }

            //Add tree-view toggle JS script
            docHtml += @"<script>
                                var toggler = document.getElementsByClassName(""namespace-toggle"");
                                var i;

                                for (i = 0; i < toggler.length; i++) {
                                  toggler[i].addEventListener(""click"", function() {
                                        this.parentElement.querySelector("".sec-namespace-inner"").classList.toggle(""sec-active"");
                                        this.classList.toggle(""namespace-toggle-down"");
                                      });
                                }
                            </script>";

            string html = _templateHtml.Replace("[BUILD-CONTENT]", docHtml);

            // Output final html to destPath
            using (FileStream stream = new FileStream(indexPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(html);
                }
            }
        }

        /// <summary>
        /// Generates an index page listing all of the types within a namespace.
        /// </summary>
        /// <param name="namespaceName"></param>
        /// <param name="objList"></param>
        /// <param name="destPath"></param>
        private void GenerateObjectIndexPage(string namespaceName, string indexPath, IEnumerable<DocObject> objList, string destPath)
        {
            destPath = $"{destPath}{namespaceName.Replace('.', '_')}.html";
            string docHtml = $"<a href=\"{indexPath.Replace("\\", "/")}\">Home</a>";
            docHtml += $"<h2>{namespaceName} Namespace</h2>{Environment.NewLine}";

            foreach (DocObject obj in objList)
                docHtml += $"{obj.HtmlName}<br/>{Environment.NewLine}";

            string html = _templateHtml.Replace("[BUILD-CONTENT]", docHtml);

            // Output final html to destPath
            using (FileStream stream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(html);
                }
            }
        }

        private void Translate(DocObject obj, string ns, Dictionary<string, List<DocObject>> namespaceList)
        {
            // Have we hit a non-namespace object?
            if (obj.Type == DocObjectType.Class || obj.Type == DocObjectType.Enum || obj.Type == DocObjectType.Struct)
            {
                if (!namespaceList.TryGetValue(ns, out List<DocObject> objects))
                {
                    objects = new List<DocObject>();
                    namespaceList.Add(ns, objects);
                }

                objects.Add(obj);
                return;
            }
            else
            {
                if (ns.Length > 0)
                    ns += ".";

                ns += obj.Name;

                foreach (DocObject member in obj.Members.Values)
                    Translate(member, ns, namespaceList);
            }
        }

        public bool IsTemplateValid { get; }
    }
}
