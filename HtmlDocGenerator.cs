using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HtmlDocGenerator
{
    public class HtmlDocGenerator
    {
        string _templateIndexHtml;
        string _templateObjHtml;
        GeneratorConfig _config;

        List<ObjectSectionGenerator> _objSectionGens;

        /// <summary>
        /// New Line.
        /// </summary>
        string _nl;

        public HtmlDocGenerator(GeneratorConfig config)
        {
            // Instantiate object section generators
            _objSectionGens = new List<ObjectSectionGenerator>();
            List<Type> secGenTypes = ReflectionHelper.FindType<ObjectSectionGenerator>();
            foreach(Type t in secGenTypes)
            {
                ObjectSectionGenerator sGen = Activator.CreateInstance(t) as ObjectSectionGenerator;
                _objSectionGens.Add(sGen);
            }

            _nl = Environment.NewLine;
            _config = config;

            // Read template html file.
            using (FileStream stream = new FileStream(_config.Template.Index, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    _templateIndexHtml = reader.ReadToEnd();
                }
            }

            using (FileStream stream = new FileStream(_config.Template.Object, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    _templateObjHtml = reader.ReadToEnd();
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
                {
                    CollateNamespaceTypes(obj, "", namespaceList);
                }
            }

            // Build per-object pages
            foreach (string ns in namespaceList.Keys)
            {
                // Filter any non-public types that were documented in the XMl source)
                namespaceList[ns] = namespaceList[ns].Where(x => x.UnderlyingType != null).ToList();

                List<DocObject> objList = namespaceList[ns];
                string nsEscaped = ns.Replace('.', '_');
                string nsDestPath = $"{destPath}{nsEscaped}";
                nsDestPath = Path.GetFullPath(nsDestPath);

                if (!Directory.Exists(nsDestPath))
                    Directory.CreateDirectory(nsDestPath);

                foreach (DocObject obj in objList)
                {
                    string objEscaped = obj.Name.Replace('<', '_').Replace('>', '_');
                    obj.PageUrl = $"{nsEscaped}/{objEscaped}.html";

                    if (obj.UnderlyingType.IsClass)
                    {
                        obj.SubType = DocObjectSubType.Class;
                    }
                    else if (obj.UnderlyingType.IsInterface)
                    {
                        obj.SubType = DocObjectSubType.Interface;
                    }
                    else if (obj.UnderlyingType.IsValueType)
                    {
                        if (obj.UnderlyingType.IsEnum)
                            obj.SubType = DocObjectSubType.Enum;
                        else
                            obj.SubType = DocObjectSubType.Struct;
                    }

                    GenerateObjectPage($"{nsDestPath}\\{objEscaped}.html", ns, obj);
                }
            }

            string indexHtml = BuildIndexTree(docs, namespaceList);
            string html = _templateIndexHtml.Replace("[BUILD-INDEX]", indexHtml);

            // TODO move JS scripts into /js directory and use config to define which ones to include

            string scriptHtml = $@"<script>
                                    let toggler = document.getElementsByClassName(""namespace-toggle"");
                                    let i;

                                    for (i = 0; i < toggler.length; i++) {{
                                      toggler[i].addEventListener(""click"", function() {{
                                            this.parentElement.querySelector("".sec-namespace-inner"").classList.toggle(""sec-active"");
                                            this.classList.toggle(""namespace-toggle-down"");
                                          }});
                                    }}
                                    
                                    let pageTargets = document.getElementsByClassName(""doc-page-target"");
                                    for (i = 0; i < pageTargets.length; i++) {{
                                            pageTargets[i].addEventListener(""click"", function(e) {{
                                                document.getElementById('content-target').src = e.target.dataset.url
                                            }});
                                    }}

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

        private string BuildIndexTree(List<DocData> docs, Dictionary<string, List<DocObject>> namespaceList)
        {
            // Output namespaces
            string html = $"<p>{_config.Index.Intro}</p>";
            List<string> nsList = namespaceList.Keys.ToList();
            nsList.Sort();

            foreach (string ns in nsList)
            {
                string nsEscaped = ns.Replace('.', '_');
                List<DocObject> objList = namespaceList[ns].Where(o => o.UnderlyingType != null).ToList();

                html += $"<div id=\"{nsEscaped}\" class=\"sec-namespace\">{_nl}";
                html += $"<span class=\"namespace-toggle\">{ns}</span><br/>{_nl}";
                html += $"    <div class=\"sec-namespace-inner\">{_nl}";
                html += GenerateObjectIndex(nsEscaped, "Classes", objList, DocObjectSubType.Class);
                html += GenerateObjectIndex(nsEscaped, "Structs", objList, DocObjectSubType.Struct);
                html += GenerateObjectIndex(nsEscaped, "Interfaces", objList, DocObjectSubType.Interface);
                html += GenerateObjectIndex(nsEscaped, "Enums", objList, DocObjectSubType.Enum);
                html += $"</div></div>{_nl}";
            }

            return html;
        }

        private string GenerateObjectIndex(string ns, string title, List<DocObject> objList, DocObjectSubType subType)
        {
            List<DocObject> filteredList = objList.Where(o => o.SubType == subType).ToList();

            if (filteredList.Count == 0)
                return "";

            string docHtml = $"<div id=\"{ns}{title}\" class=\"sec-namespace sec-namespace-noleft\">{_nl}";
            docHtml += $"<span class=\"namespace-toggle\">{title}</span><br/>{_nl}";
            docHtml += $"    <div class=\"sec-namespace-inner\">{_nl}";

            docHtml += $"<table class=\"sec-obj-index\"><thead><tr>{_nl}";
            docHtml += $"<th class=\"col-type-icon\"></th>{_nl}";
            docHtml += $"<th class=\"col-type-name\"></th>{_nl}";
            docHtml += $"</tr></thead><tbody>{_nl}";
            foreach (DocObject obj in filteredList)
            {
                string summary = string.IsNullOrWhiteSpace(obj.Summary) ? "" : obj.Summary;
                if (_config.Summary.MaxLength > 0 && summary.Length > _config.Summary.MaxLength)
                {
                    summary = summary.Substring(0, _config.Summary.MaxLength);
                    summary += $"...<a href=\"#\">{_config.Summary.ReadMore}</a>{_nl}";
                }

                string htmlIcon = GetIconHtml(obj);
                docHtml += $"   <tr id=\"{ns}-{obj.HtmlName}\" class=\"sec-namespace-obj\">{_nl}";
                docHtml += $"       <td>{htmlIcon}</td>{_nl}";
                docHtml += $"       <td><span class=\"doc-page-target\" data-url=\"{obj.PageUrl}\">{obj.HtmlName}</span></td>{_nl}";
                docHtml += $"    </tr>{_nl}";
            }

            docHtml += $"</tbody></table>{_nl}";
            docHtml += $"</div></div>{_nl}";
            return docHtml;
        }

        private string GetIconHtml(DocObject obj, string pathPrefix = "")
        {
            string iconName = obj.SubType.ToString().ToLower();
            string html = "&nbsp;";

            if (!string.IsNullOrWhiteSpace(iconName))
            {
                if(_config.Icons.TryGetValue(iconName.ToLower(), out string iconPath))
                    html = $"<img src=\"{pathPrefix}{iconPath}\"/>";
            }

            return html;
        }

        private void GenerateObjectPage(string destPath, string ns, DocObject obj)
        {
            string docHtml = "";

            foreach (ObjectSectionGenerator sGen in _objSectionGens)
            {
                docHtml += sGen.Generate(obj);
                docHtml += "<br/>";
            }

            string html = _templateObjHtml
                .Replace("[NAMESPACE]", ns)
                .Replace("[TITLE]", obj.HtmlName)
                .Replace("[ICON]", GetIconHtml(obj, "../"))
                .Replace("[SUMMARY]", obj.Summary)
                .Replace("[BUILD-CONTENT]", docHtml);


            using (FileStream stream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(html);
                }
            }

            Console.WriteLine($"Created page for {ns}.{obj.Name}: {destPath}");
        }

        private void CollateNamespaceTypes(DocObject obj, string ns, Dictionary<string, List<DocObject>> namespaceList)
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
                    CollateNamespaceTypes(member, ns, namespaceList);
            }
        }
    }
}
