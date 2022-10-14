using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HtmlDocGenerator
{
    public class DocParser
    {
        Dictionary<string, DocObjectType> _typeKeys = new Dictionary<string, DocObjectType>()
        {
            ["T"] = DocObjectType.ObjectType,
            ["E"] = DocObjectType.Event,
            ["P"] = DocObjectType.Property,
            ["F"] = DocObjectType.Field,
            ["M"] = DocObjectType.Method
        };

        public DocData ParseXml(FileStream stream)
        {
            DocData doc = new DocData();
            DocNode root = new DocNode()
            {
                Type = DocNodeType.Document
            };

            using (XmlReader reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);

                ParseNode(doc, xml, root);
            }

            return doc;
        }

        public void ScanAssembly(HtmlContext context, DocData doc, Assembly assembly)
        {
            try
            {
                context.Log($"Retrieving type list from '{doc.AssemblyName}'");
                Type[] aTypes = assembly.GetExportedTypes();
                context.Log($"Retrieved {aTypes.Length} public types from '{doc.AssemblyName}'");

                foreach (Type t in aTypes)
                {
                    DocObjectType objType = DocObjectType.ObjectType;
                    DocObject obj = ParseTypeName(doc, t.FullName, objType);
                    obj.UnderlyingType = t;

                    if (t.IsGenericType)
                        obj.Name = HtmlHelper.GetHtmlName(t);
                }
            }
            catch (Exception ex)
            {
                context.Error($"Failed to retrieve public types from '{doc.AssemblyName}': {ex.Message}");
            }
        }

        private void ParseNode(DocData doc, XmlNode xmlNode, DocNode docNode)
        {
            docNode.Name = xmlNode.Name;

            switch (docNode.Name)
            {
                case "assembly":
                    XmlNode aName = xmlNode["name"];
                    doc.AssemblyName = aName.InnerText;
                    break;

                case "member":
                    ParseMemberNode(doc, xmlNode, docNode);
                    break;

                case "summary":
                    if (docNode.Parent != null)
                    {
                        docNode.Parent.Object.Summary = xmlNode.InnerXml;
                    }
                    break;
            }

            // Parse child nodes
            foreach(XmlNode child in xmlNode.ChildNodes)
            {
                DocNode docChild = docNode.AddChild("");
                ParseNode(doc, child, docChild);
            }
        }

        private void ParseMemberNode(DocData doc, XmlNode xmlNode, DocNode docNode)
        {
            XmlAttribute attName = xmlNode.Attributes["name"];

            if (attName == null)
                return;

            string typeName = attName.Value;
            string typeKey = typeName.Substring(0, 1);
            DocObjectType objectType;

            if (!_typeKeys.TryGetValue(typeKey, out objectType))
                return;

            typeName = typeName.Substring(2, typeName.Length - 2);

            docNode.Object = ParseTypeName(doc, typeName, objectType);
        }

        private DocObject ParseTypeName(DocData doc, string typeName, DocObjectType objectType)
        {
            string[] mParts = typeName.Split('.');

            // Breakdown the member name into parts so we can put the member inside the correct namespace or type.

            DocObject parent = doc;
            for (int i = 0; i < mParts.Length; i++)
            {
                string part = mParts[i];

                DocObjectType newObjType = objectType;
                if (i != mParts.Length - 1)
                    newObjType = DocObjectType.None;

                if (!parent.Members.TryGetValue(part, out DocObject obj))
                {
                    obj = parent.AddMember(part, newObjType);
                }
                else
                {
                    if (newObjType != DocObjectType.None)
                        obj.Type = newObjType;
                }

                parent = obj;
            }

            return parent;
        }
    }
}
