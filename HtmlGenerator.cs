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

        /// <summary>
        /// New Line.
        /// </summary>
        string _nl;

        public HtmlGenerator(GeneratorConfig config)
        {
            _nl = Environment.NewLine;
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
            string indexHtml = BuildIndexTree(docs);
            string html = _templateHtml.Replace("[BUILD-INDEX]", indexHtml);


            string scriptHtml = @"<script>
                                var toggler = document.getElementsByClassName(""namespace-toggle"");
                                var i;

                                for (i = 0; i < toggler.length; i++) {
                                  toggler[i].addEventListener(""click"", function() {
                                        this.parentElement.querySelector("".sec-namespace-inner"").classList.toggle(""sec-active"");
                                        this.classList.toggle(""namespace-toggle-down"");
                                      });
                                }
                            </script>";
            html = html.Replace("[SCRIPTS]", scriptHtml);

            // Output final html to destPath
            using (FileStream stream = new FileStream(indexPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(html);
                }
            }
        }

        private string BuildIndexTree(List<DocData> docs)
        {
            Dictionary<string, List<DocObject>> namespaceList = new Dictionary<string, List<DocObject>>();
            foreach (DocData doc in docs)
            {
                foreach (DocObject obj in doc.Members.Values)
                    Translate(obj, "", namespaceList);
            }

            // Output namespaces
            string html = $"<p>{_config.Index.Intro}</p>";
            List<string> nsList = namespaceList.Keys.ToList();
            nsList.Sort();

            foreach (string ns in nsList)
            {
                string nsEscaped = ns.Replace('.', '_');
                List<DocObject> objList = namespaceList[ns].Where(o => o.UnderlyingType != null).ToList();
                List<DocObject> objClasses = objList.Where(o => o.UnderlyingType.IsClass).ToList();
                List<DocObject> objInterfaces = objList.Where(o => o.UnderlyingType.IsInterface).ToList();
                List<DocObject> objStructs = objList.Where(o => o.UnderlyingType.IsValueType && !o.UnderlyingType.IsEnum).ToList();
                List<DocObject> objEnums = objList.Where(o => o.UnderlyingType.IsValueType && o.UnderlyingType.IsEnum).ToList();

                html += $"<div id=\"{nsEscaped}\" class=\"sec-namespace\">{_nl}";
                html += $"<span class=\"namespace-toggle\">{ns}</span><br/>{_nl}";
                html += $"    <div class=\"sec-namespace-inner\">{_nl}";
                html += GenerateObjectIndex(nsEscaped, "Classes", "img/object.png", objClasses);
                html += GenerateObjectIndex(nsEscaped, "Structs", "", objStructs);
                html += GenerateObjectIndex(nsEscaped, "Interfaces", "", objInterfaces);
                html += GenerateObjectIndex(nsEscaped, "Enums", "", objEnums);
                html += $"</div></div>{_nl}";
            }

            return html;
        }

        private string GenerateObjectIndex(string ns, string title, string iconUrl, List<DocObject> objList)
        {
            if (objList.Count == 0)
                return "";

            string docHtml = $"<div id=\"{ns}{title}\" class=\"sec-namespace sec-namespace-noleft\">{_nl}";
            docHtml += $"<span class=\"namespace-toggle\">{title}</span><br/>{_nl}";
            docHtml += $"    <div class=\"sec-namespace-inner\">{_nl}";

            docHtml += $"<table class=\"sec-obj-index\"><thead><tr>{_nl}";
            docHtml += $"<th class=\"col-type-icon\"></th>{_nl}";
            docHtml += $"<th class=\"col-type-name\"></th>{_nl}";
            docHtml += $"</tr></thead><tbody>{_nl}";
            foreach (DocObject obj in objList)
            {
                string summary = string.IsNullOrWhiteSpace(obj.Summary) ? "" : obj.Summary;
                if (_config.Summary.MaxLength > 0 && summary.Length > _config.Summary.MaxLength)
                {
                    summary = summary.Substring(0, _config.Summary.MaxLength);
                    summary += $"...<a href=\"#\">{_config.Summary.ReadMore}</a>{_nl}";
                }

                string iconHtml = iconUrl.Length > 0 ? $"<img src=\"{iconUrl}\"/>" : "&nbsp;";
                docHtml += $"   <tr id=\"{ns}-{obj.HtmlName}\" class=\"sec-namespace-obj\">{_nl}";
                docHtml += $"       <td>{iconHtml}</td>{_nl}";
                docHtml += $"       <td>{obj.HtmlName}</td>{_nl}";
                docHtml += $"    </tr>{_nl}";
            }

            docHtml += $"</tbody></table>{_nl}";
            docHtml += $"</div></div>{_nl}";
            return docHtml;
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
            docHtml += $"<h2>{namespaceName} Namespace</h2>{_nl}";

            foreach (DocObject obj in objList)
                docHtml += $"{obj.HtmlName}<br/>{_nl}";

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
            if (obj.Type == DocObjectType.ObjectType)
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
