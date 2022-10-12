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
            foreach(Type t in secGenTypes)
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
        public void Generate(HtmlContext context, List<DocData> docs, string destPath, string indexPath)
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

                    GenerateObjectPage(context, $"{nsDestPath}\\{objEscaped}.html", ns, obj);
                }
            }

            string indexHtml = BuildIndexTree(context, docs, namespaceList);
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

        private string BuildIndexTree(HtmlContext context, List<DocData> docs, Dictionary<string, List<DocObject>> namespaceList)
        {
            // Output namespaces
            string html = $"<p>{context.Index.Intro}</p>";
            List<string> nsList = namespaceList.Keys.ToList();
            nsList.Sort();

            foreach (string ns in nsList)
            {
                string nsEscaped = ns.Replace('.', '_');
                List<DocObject> objList = namespaceList[ns].Where(o => o.UnderlyingType != null).ToList();

                html += GenerateTreeBranch(nsEscaped, ns, () =>
                {
                    string innerHtml = GenerateObjectIndex(context, nsEscaped, "Classes", objList, DocObjectSubType.Class);
                    innerHtml += GenerateObjectIndex(context, nsEscaped, "Structs", objList, DocObjectSubType.Struct);
                    innerHtml += GenerateObjectIndex(context, nsEscaped, "Interfaces", objList, DocObjectSubType.Interface);
                    innerHtml += GenerateObjectIndex(context, nsEscaped, "Enums", objList, DocObjectSubType.Enum);
                    return innerHtml;
                });
            }

            return html;
        }

        private string GenerateObjectIndex(HtmlContext context, string ns, string title, List<DocObject> objList, DocObjectSubType subType)
        {
            List<DocObject> filteredList = objList.Where(o => o.SubType == subType).ToList();

            if (filteredList.Count == 0)
                return "";

            return GenerateTreeBranch(ns, title, () =>
            {
                string html = "";
                foreach (DocObject obj in filteredList)
                {
                    if (obj.TypeMembers.Length == 0 || obj.UnderlyingType.IsEnum)
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
                        html += GenerateTreeBranch(nsObj, obj.HtmlName, () =>
                        {
                            string innerHtml = "";
                            foreach (ObjectSectionGenerator secGen in _objSectionGens)
                            {
                                string secHtml = secGen.GenerateIndexTreeItems(context, nsObj, obj);

                                if (secHtml.Length > 0)
                                {
                                    string secTitle = secGen.GetTitle();
                                    string nsSec = $"{nsObj}{secTitle}";

                                    innerHtml += GenerateTreeBranch(nsSec, secTitle, secHtml, 3);
                                }
                            }

                            return innerHtml;
                        }, 2);
                        
                    }
                }

                return html;
            }, 1);
        }

        private string GenerateTreeBranch(string ns, string title, Func<string> contentCallback, int depth = 0)
        {
            string contentHtml = contentCallback?.Invoke() ?? "";
            return GenerateTreeBranch(ns, title, contentHtml, depth);
        }

        private string GenerateTreeBranch(string ns, string title, string contentHtml, int depth = 0)
        {
            string docHtml = $"<div id=\"{ns}{title}\" class=\"sec-namespace sec-namespace{(depth > 0 ? "-noleft" : "")}\">{_nl}";
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
