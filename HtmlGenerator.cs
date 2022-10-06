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

        public HtmlGenerator(string templatePath)
        {
            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"Template not found: {templatePath}");
                return;
            }

            // Read template html file.
            using (FileStream stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
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
            string docHtml = "";
            List<string> nsList = namespaceList.Keys.ToList();
            nsList.Sort();

            foreach (string ns in nsList)
            {
                docHtml += $"<a href=\"docs\\{ns.Replace('.', '_')}.html\">{ns}</a><br/>";
                GenerateObjectIndexPage(ns, namespaceList[ns], destPath);
            }

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
        private void GenerateObjectIndexPage(string namespaceName, IEnumerable<DocObject> objList, string destPath)
        {
            destPath = $"{destPath}{namespaceName.Replace('.', '_')}.html";
            string docHtml = $"<h2>{namespaceName} Namespace</h2>{Environment.NewLine}";


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
