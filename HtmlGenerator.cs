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
        public void Generate(DocData doc, string templatePath, string destPath)
        {
            destPath = $"{destPath}{doc.Assembly.Name}.html";

            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"Template not found: {templatePath}");
                return;
            }

            // Read template html file.
            string html = "";
            using (FileStream stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
            }

            string docHtml = Translate(doc);
            html = html.Replace("[BUILD-CONTENT]", docHtml);

            // Output final html to destPath
            using (FileStream stream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(html);
                }
            }
        }

        private string Translate(DocData doc)
        {
            string html = $"<h1>{doc.Assembly.Name}</h1>";
            html += "<hr/>";

            // Build namespace list
            Dictionary<string, List<DocObject>> namespaceList = new Dictionary<string, List<DocObject>>();
            foreach (DocObject obj in doc.Members.Values)
                TranslateNamespace(obj, "", namespaceList);

            // Output namespaces and their objects
            foreach(string ns in namespaceList.Keys)
            {
                html += $"<h4>{ns}</h4>";
                List<DocObject> objects = namespaceList[ns];
                foreach (DocObject obj in objects)
                    html += $"{obj.Name}<br/>";
            }

            return html;
        }

        private void TranslateNamespace(DocObject obj, string ns, Dictionary<string, List<DocObject>> namespaceList)
        {
            // Have we hit a non-namespace object?
            if (obj.Type == DocObjectType.UnspecifiedType)
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
                    TranslateNamespace(member, ns, namespaceList);
            }
        }
    }
}
