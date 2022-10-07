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
            ["T"] = DocObjectType.UnspecifiedType,
            ["E"] = DocObjectType.Event,
            ["P"] = DocObjectType.Property,
            ["F"] = DocObjectType.Field,
            ["M"] = DocObjectType.Method
        };

        public DocParser()
        {

        }

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

        public void ScanAssembly(DocData doc, Assembly assembly)
        {
            try
            {
                Console.WriteLine($"Retrieving type list from '{doc.AssemblyName}'");
                Type[] aTypes = assembly.GetExportedTypes();
                Console.WriteLine($"Retrieved {aTypes.Length} public types from '{doc.AssemblyName}'");

                foreach (Type t in aTypes)
                {
                    DocObjectType objType = DocObjectType.None;
                    if (t.IsValueType)
                        objType = t.IsEnum ? DocObjectType.Enum : DocObjectType.Struct;
                    else
                        objType = DocObjectType.Class;

                    DocObject obj = ParseTypeName(doc, t.FullName, objType);
                    obj.UnderlyingType = t;

                    if (t.IsGenericType)
                        obj.Name = ParseTypeName(t);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve public types from '{doc.AssemblyName}': {ex.Message}");
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

        private string ParseTypeName(Type t)
        {
            string[] gParts = t.Name.Split('`');
            string name = gParts[0];
            Type[] gTypes = t.GetGenericArguments();

            if (gTypes.Length > 0)
            {
                name += '<';
                for (int i = 0; i < gTypes.Length; i++)
                {
                    Type gType = gTypes[i];
                    name += $"{(i == 0 ? "" : ", ")}{ParseTypeName(gType)}";
                }
                name += '>';
            }

            return name;
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
