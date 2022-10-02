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

        public void ScanAssembly(DocData data, Assembly assembly)
        {
            Type[] aTypes = assembly.GetExportedTypes();

        }

        private void ParseNode(DocData doc, XmlNode xmlNode, DocNode docNode)
        {
            docNode.Name = xmlNode.Name;

            switch (docNode.Name)
            {
                case "assembly":
                    XmlNode aName = xmlNode["name"];
                    doc.Assembly = new DocAssembly(aName.InnerText);
                    break;

                case "member":
                    ParseMemberNode(doc, xmlNode, docNode);
                    break;
            }

            // Parse child nodes
            foreach(XmlNode child in xmlNode.ChildNodes)
            {
                DocNode docChild = new DocNode();
                docNode.Children.Add(docChild);

                ParseNode(doc, child, docChild);
            }
        }

        private void ParseMemberNode(DocData doc, XmlNode xmlNode, DocNode docNode)
        {
            XmlAttribute attName = xmlNode.Attributes["name"];

            if (attName == null)
                return;

            string memberName = attName.Value;
            string typeKey = memberName.Substring(0, 1);
            DocObjectType objectType = DocObjectType.None;

            if (!_typeKeys.TryGetValue(typeKey, out objectType))
                return;

            memberName = memberName.Substring(2, memberName.Length - 2);
            string[] mParts = memberName.Split('.');

            // Breakdown the member name into parts so we can put the member inside the correct namespace or type.

            DocObject parent = doc;
            for(int i = 0; i < mParts.Length; i++)
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
                    if(newObjType != DocObjectType.None)
                        obj.Type = newObjType;
                }

                parent = obj;
            }
        }
    }
}
