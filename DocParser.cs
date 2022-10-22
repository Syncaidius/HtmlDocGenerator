using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HtmlDocGenerator
{
    public class DocParser
    {
        Dictionary<char, XmlMemberType> _typeKeys = new Dictionary<char, XmlMemberType>()
        {
            ['T'] = XmlMemberType.ObjectType,
            ['E'] = XmlMemberType.Event,
            ['P'] = XmlMemberType.Property,
            ['F'] = XmlMemberType.Field,
            ['M'] = XmlMemberType.Method,
            ['!'] = XmlMemberType.Invalid
        };

        /// <summary>
        /// Parses a summary xml file and adds it's information to the provided <see cref="HtmlContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="HtmlContext"/> to which the parsed XML will be added.</param>
        /// <param name="xmlPath">The path of the XML summary file to be parsed.</param>
        /// <param name="assemblyPath">A custom path to the assembly file from which the xml summary file was generated.</param>
        public void LoadXml(HtmlContext context, string xmlPath, string assemblyPath = null)
        {
            FileInfo info = new FileInfo(xmlPath);
            if (!info.Exists)
            {
                Console.WriteLine($"The specified file does not exist: {info.FullName}");
                return;
            }

            using (FileStream stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    reader.MoveToContent();
                    XmlDocument xml = new XmlDocument();
                    xml.Load(reader);

                    XmlNode docRoot = xml["doc"];
                    if (docRoot == null) {
                        context.Error($"The root 'doc' node was not found. Unable to continue.");
                        return;
                    }

                    //LoadXmlAssembly(context, docNode, info, assemblyPath);
                    XmlNode xAssembly = docRoot["assembly"];

                    if (xAssembly != null)
                    {
                        if (!context.Assemblies.TryGetValue(xAssembly.InnerText, out DocAssembly da))
                        {
                            da = new DocAssembly();
                            da.Name = xAssembly.InnerText;
                            da.FilePath = assemblyPath;

                            if (string.IsNullOrWhiteSpace(da.FilePath))
                                da.FilePath = $"{info.Directory}\\{da.Name}.dll";

                            if (!da.Load(context))
                                return;

                            context.Assemblies.Add(da.Name, da);
                        }

                        if(da.XmlRoot == null)
                        {
                            // Run through the public types in the assembly.
                            da.XmlRoot = docRoot;
                            context.Log($"Retrieving type list for '{da.Name}'");
                            Type[] aTypes = da.Assembly.GetExportedTypes();
                            context.Log($"Retrieved {aTypes.Length} public types for '{da.Name}'");

                            foreach (Type t in aTypes)
                            {
                                DocObject obj = context.CreateObject(t);
                                obj.UnderlyingType = t;

                                if (t.IsGenericType)
                                    obj.Name = HtmlHelper.GetHtmlName(t);
                            }
                        }
                    }
                }
            }
        }

        public void ParseXml(HtmlContext context, DocAssembly assembly)
        {
            XmlNode xMembers = assembly.XmlRoot["members"];
            if (xMembers != null)
            {
                foreach (XmlNode node in xMembers.ChildNodes)
                {
                    if (node.Name != "member")
                        continue;

                    ParseMember(context, node);
                }
            }
        }

        private void ParseMember(HtmlContext context, XmlNode memberNode)
        {
            XmlAttribute attName = memberNode.Attributes["name"];
            if (attName == null)
            {
                context.Error($"No 'name' attribute found for member. Skipping");
                return;
            }

            DocElement el = ParseXmlName(context, attName.Value, out XmlMemberType mType, out string mName);

            if (el != null)
            {
                foreach (XmlNode sumNode in memberNode)
                {
                    switch (sumNode.Name)
                    {
                        case "summary":
                            el.Summary = ParseSummary(context, sumNode.InnerXml);
                            break;

                        case "param":
                            if (el is DocMethodMember method)
                            {
                                XmlAttribute attParamName = sumNode.Attributes["name"];
                                if (attParamName != null)
                                {
                                    if (method.ParametersByName.TryGetValue(attParamName.Value, out DocParameter dp))
                                        dp.Summary = ParseSummary(context, sumNode.InnerXml);
                                }
                            }
                            break;
                    }
                }
            }
        }

        private DocElement ParseXmlName(HtmlContext context, string typeName, out XmlMemberType mType, out string name)
        {
            typeName = typeName.Replace("&lt;", "<").Replace("&gt;", ">");
            string ns = "";
            string nsPrev = "";

            mType = XmlMemberType.None;
            name = "";
            char startsWith = typeName[0];
            if (!_typeKeys.TryGetValue(startsWith, out mType))
            {
                context.Error($"Invalid type-name prefix '{startsWith}' for '{typeName}'");
                return null;
            }

            typeName = typeName.Substring(2); // Get typeName without the [type]: prefix

            if (mType == XmlMemberType.Invalid)
            {
                context.Error($"Invalid type '{typeName}' detected. Cannot parse XML name. Starts with '{startsWith}' operator");
                name = typeName;
                return null;
            }

            string objectName = "";
            string memberName = "";

            string prev = "";
            string cur = "";

            List<Type> methodParams = new List<Type>();

            for (int i = 0; i < typeName.Length; i++)
            {
                char c = typeName[i];
                switch (c)
                {
                    case '.':
                        // Ignore namespace separator if member name is already defined
                        // because these will be parameter namespace separators.
                        if (memberName.Length == 0)
                        {
                            nsPrev = ns;
                            ns += ns.Length > 0 ? $".{cur}" : cur;
                            prev = cur;
                            cur = "";
                        }
                        else
                        {
                            cur += ".";
                        }
                        break;

                    case ',':
                        Type t = Type.GetType(cur);
                        methodParams.Add(t);
                        prev = cur;
                        cur = "";
                        break;

                    case '#':
                        objectName = prev;
                        ns = nsPrev;
                        break;

                    case '(':
                        memberName = cur;
                        prev = cur;
                        cur = "";
                        break;

                    case ')':
                        prev = cur;
                        cur = "";
                        break;

                    default:
                        cur += c;
                        break;
                }
            }

            if (memberName.Length == 0)
                memberName = cur;

            DocElement el = null;
            if (mType != XmlMemberType.ObjectType)
            {
                if (objectName.Length == 0)
                    objectName = prev;
                ns = nsPrev;

                DocObject parentObj = context.GetObject(ns, objectName);
                if (parentObj != null)
                    el = parentObj.GetMember<DocMethodMember>(memberName, methodParams?.ToArray());
            }
            else
            {
                el = context.GetObject(ns, memberName);
            }


            return el;
        }

        private string ParseSummary(HtmlContext context, string xmlText, XmlDocument xmlDoc = null)
        {
            xmlDoc = xmlDoc ?? new XmlDocument();
            int nodeStart = 0;

            bool tagStarted = false;
            bool inOpenNode = false; // True if a <tag> has been parsed without a closing </tag>.
            char cPrev = '\0';
            string summary = "";

            for(int i = 0; i < xmlText.Length; i++)
            {
                char c = xmlText[i];

                switch (c)
                {
                    case '<':
                        if (!tagStarted)
                        {
                            if (inOpenNode)
                            {
                                // Check if we're starting a closing </tag>.
                                if (i < xmlText.Length - 1 && xmlText[i + 1] != '/')
                                {
                                    int closeIndex = xmlText.IndexOf('>', i, xmlText.Length - i);
                                    if (closeIndex > -1)
                                    {
                                        string nXml = xmlText.Substring(i, xmlText.Length - closeIndex);
                                        string nText = ParseSummary(context, nXml, xmlDoc);
                                        xmlText = xmlText.Replace(nXml, nText);
                                        summary += nText;
                                    }
                                }
                            }
                            else
                            {
                                nodeStart = i;
                                tagStarted = true;
                            }
                        }
                        break;

                    case '>':
                        if (tagStarted)
                        {
                            // Consider a "/>" inline tag terminator.
                            if (inOpenNode || cPrev == '/') 
                            {
                                int len = (i - nodeStart) + 1;
                                string nXml = xmlText.Substring(nodeStart, len);
                                string nText = ParseSummaryXml(context, nXml, xmlDoc);
                                xmlText = xmlText.Replace(nXml, nText);

                                
                                summary += nText;
                                i = nodeStart + (nText.Length-1);
                                c = xmlText[i]; // Recheck current char.
                                inOpenNode = false;
                            }

                            tagStarted = false;
                        }
                        break;

                    default:
                        if (!inOpenNode && !tagStarted)
                            summary += c;
                        break;
                }

                cPrev = c;
            }

            return summary;
        }

        private string ParseSummaryXml(HtmlContext context, string xml, XmlDocument xmlDoc)
        {
            xmlDoc.LoadXml(xml);
            XmlNode subNode = xmlDoc.FirstChild;
            string summary = "";

            switch (subNode.Name)
            {
                case "see":
                    XmlAttribute attCRef = subNode.Attributes["cref"];
                    if (attCRef != null)
                    {
                        DocElement refObj = ParseXmlName(context, attCRef.Value, out XmlMemberType mType, out string mName);
                        if (refObj != null)
                            summary = $"<a href=\"{refObj.PageUrl}\">{refObj.Name}</a>";
                        else if (mType == XmlMemberType.Invalid)
                            summary = $"<b class=\"obj-invalid\" title=\"Invalid object name\">{mName}</b>";
                    }
                    break;
            }

            return summary;
        }
    }
}
