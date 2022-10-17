using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HtmlDocGenerator
{
    public class DocGenerator
    {
        List<ObjectSectionGenerator> _objSectionGens;

        /// <summary>
        /// New Line.
        /// </summary>
        string _nl;

        public DocGenerator()
        {
            // Instantiate object section generators
            _objSectionGens = new List<ObjectSectionGenerator>();
            List<Type> secGenTypes = ReflectionHelper.FindType<ObjectSectionGenerator>();
            foreach (Type t in secGenTypes)
            {
                ObjectSectionGenerator sGen = Activator.CreateInstance(t) as ObjectSectionGenerator;
                _objSectionGens.Add(sGen);
            }

            _nl = Environment.NewLine;
        }

        /// <summary>
        /// Generates an index page listing all of the available namespaces in the documented assemblies.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="indexPath"></param>
        public void Generate(HtmlContext context, string destPath, string indexPath)
        {
            // Build per-object pages
            foreach (string ns in context.Namespaces.Keys)
            {
                // Filter any non-public types that were documented in the XML source)
                context.Namespaces[ns] = context.Namespaces[ns].Where(x => x.UnderlyingType != null).ToList();

                List<DocObject> objList = context.Namespaces[ns];
                string nsPath = context.GetFileName(ns);
                string nsDestPath = $"{destPath}{nsPath}";
                nsDestPath = Path.GetFullPath(nsDestPath);

                if (!Directory.Exists(nsDestPath))
                    Directory.CreateDirectory(nsDestPath);

                foreach (DocObject obj in objList)
                {
                    string objEscaped = context.GetFileName(obj.Name);
                    obj.PageUrl = $"{nsPath}/{objEscaped}.html";

                    GenerateObjectPage(context, $"{nsDestPath}\\{objEscaped}.html", ns, obj);
                }
            }

            string indexHtml = BuildIndexTree(context, context.Namespaces);
            string html = context.Template.IndexHtml.Replace("[BUILD-INDEX]", indexHtml);

            // TODO move JS scripts into /js directory and use config to define which ones to include

            string scriptHtml = "";
            foreach (string scriptPath in context.Scripts)
            {
                string fullScriptPath = Path.GetFullPath(scriptPath);
                string scriptText = "";

                using (FileStream stream = new FileStream(fullScriptPath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(stream))
                        scriptText = reader.ReadToEnd();
                }
                scriptHtml += $"<script>{scriptText}</script>";
            }

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

        private string BuildIndexTree(HtmlContext context, Dictionary<string, List<DocObject>> namespaceList)
        {
            // Output namespaces
            string html = $"<p>{context.Index.Intro}</p>";
            List<string> nsList = namespaceList.Keys.ToList();
            nsList.Sort();

            foreach (string ns in nsList)
            {
                List<DocObject> objList = namespaceList[ns];

                html += GenerateTreeBranch(context, ns, ns, () =>
                {
                    string innerHtml = GenerateObjectIndex(context, ns, "Classes", objList, DocObjectType.Class);
                    innerHtml += GenerateObjectIndex(context, ns, "Structs", objList, DocObjectType.Struct);
                    innerHtml += GenerateObjectIndex(context, ns, "Interfaces", objList, DocObjectType.Interface);
                    innerHtml += GenerateObjectIndex(context, ns, "Enums", objList, DocObjectType.Enum);
                    return innerHtml;
                });
            }

            return html;
        }

        private string GenerateObjectIndex(HtmlContext context, string ns, string title, List<DocObject> objList, DocObjectType objType)
        {
            List<DocObject> filteredList = objList.Where(o => o.DocType == objType).ToList();

            if (filteredList.Count == 0)
                return "";

            return GenerateTreeBranch(context, ns, title, () =>
            {
                string html = "";
                foreach (DocObject obj in filteredList)
                {
                    if (obj.MembersByType.Count == 0 || obj.UnderlyingType.IsEnum)
                    {
                        html += $"<table class=\"sec-obj-index\"><thead><tr>{_nl}";
                        html += $"<th class=\"col-type-icon\"></th>{_nl}";
                        html += $"<th class=\"col-type-name\"></th>{_nl}";
                        html += $"</tr></thead><tbody>{_nl}";
                        html += $"   <tr id=\"{ns}-{obj.HtmlName}\" class=\"sec-namespace-obj\">{_nl}";
                        string htmlIcon = context.GetIcon(obj);
                        html += $"       <td>{htmlIcon}</td>{_nl}";
                        html += $"       <td><span class=\"doc-page-target\" data-url=\"{obj.PageUrl}\">{obj.HtmlName}</span></td>{_nl}";
                        html += $"    </tr>{_nl}";
                        html += $"</tbody></table>{_nl}";
                    }
                    else
                    {
                        string nsObj = $"{ns}{title}";
                        html += GenerateTreeBranch(context, nsObj, obj.HtmlName, () =>
                        {
                            string innerHtml = "";
                            foreach (ObjectSectionGenerator secGen in _objSectionGens)
                            {
                                string secHtml = secGen.GenerateIndexTreeItems(context, nsObj, obj);

                                if (secHtml.Length > 0)
                                {
                                    string secTitle = secGen.GetTitle();
                                    string nsSec = $"{nsObj}{secTitle}";

                                    innerHtml += GenerateTreeBranch(context, nsSec, secTitle, secHtml, 3);
                                }
                            }

                            return innerHtml;
                        }, 2);

                    }
                }

                return html;
            }, 1);
        }

        private string GenerateTreeBranch(HtmlContext context, string ns, string title, Func<string> contentCallback, int depth = 0)
        {
            string contentHtml = contentCallback?.Invoke() ?? "";
            return GenerateTreeBranch(context, ns, title, contentHtml, depth);
        }

        private string GenerateTreeBranch(HtmlContext context, string ns, string title, string contentHtml, int depth = 0)
        {
            string nsPath = context.GetFileName(ns);
            string docHtml = $"<div id=\"{nsPath}{title}\" class=\"sec-namespace sec-namespace{(depth > 0 ? "-noleft" : "")}\">{_nl}";
            docHtml += $"<span class=\"namespace-toggle\">{title}</span><br/>{_nl}";
            docHtml += $"    <div class=\"sec-namespace-inner\">{_nl}";
            docHtml += contentHtml;
            docHtml += $"</div></div>{_nl}";
            return docHtml;
        }

        private void GenerateObjectPage(HtmlContext context, string destPath, string ns, DocObject obj)
        {
            string docHtml = "";

            foreach (ObjectSectionGenerator sGen in _objSectionGens)
                docHtml += sGen.Generate(context, ns, obj);

            string html = context.Template.ObjectHtml
                .Replace("[NAMESPACE]", ns)
                .Replace("[TITLE]", obj.HtmlName)
                .Replace("[ICON]", context.GetIcon(obj, "../"))
                .Replace("[SUMMARY]", obj.Summary)
                .Replace("[BUILD-CONTENT]", docHtml);


            using (FileStream stream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(html);
                }
            }

            context.Log($"Created page for {ns}.{obj.Name}: {destPath}");
        }
    }
}
