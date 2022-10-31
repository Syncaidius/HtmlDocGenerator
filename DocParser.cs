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
        Regex _regexHttp = new Regex("(\\b(http|ftp|https):(\\/\\/|\\\\\\\\)[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&:/~\\+#]*[\\w\\-\\@?^=%&/~\\+#])?|\\bwww\\.[^\\s])");
        Dictionary<char, DocObjectType> _typeKeys = new Dictionary<char, DocObjectType>()
        {
            ['T'] = DocObjectType.ObjectType,
            ['E'] = DocObjectType.Event,
            ['P'] = DocObjectType.Property,
            ['F'] = DocObjectType.Field,
            ['M'] = DocObjectType.Method,
            ['!'] = DocObjectType.Invalid
        };


        /// <summary>
        /// Parses a summary xml file and adds it's information to the provided <see cref="DocContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="DocContext"/> to which the parsed XML will be added.</param>
        /// <param name="xmlPath">The path of the XML summary file to be parsed.</param>
        /// <param name="assemblyPath">A custom path to the assembly file from which the xml summary file was generated.</param>
        public void LoadXml(DocContext context, string xmlPath, string assemblyPath = null)
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
                            }
                        }
                    }
                }
            }
        }

        public void Parse(DocContext context, string destPath)
        {
            // Parse assembly XML members.
            foreach (DocAssembly a in context.Assemblies.Values)
            {
                XmlNode xMembers = a.XmlRoot["members"];
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
        }

        private void ParseMember(DocContext context, XmlNode memberNode)
        {
            XmlAttribute attName = memberNode.Attributes["name"];
            if (attName == null)
            {
                context.Error($"No 'name' attribute found for member. Skipping");
                return;
            }

            DocElement el = ParseXmlName(context, attName.Value, out DocObjectType mType, out string mName);

            if (el != null)
            {
                foreach (XmlNode sumNode in memberNode)
                {
                    switch (sumNode.Name)
                    {
                        case "summary":
                            el.Summary = ParseSummary(context, sumNode.InnerXml);
                            break;

                        case "remarks":
                            el.Remark = ParseSummary(context, sumNode.InnerXml);
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

        private DocElement ParseXmlName(DocContext context, string typeName, out DocObjectType mType, out string name)
        {
            typeName = typeName.Replace("&lt;", "<").Replace("&gt;", ">");
            string ns = "";
            string nsPrev = "";

            mType = DocObjectType.Unknown;
            name = "";
            char startsWith = typeName[0];
            if (!_typeKeys.TryGetValue(startsWith, out mType))
            {
                context.Error($"Invalid type-name prefix '{startsWith}' for '{typeName}'");
                return null;
            }

            typeName = typeName.Substring(2); // Get typeName without the [type]: prefix

            if (mType == DocObjectType.Invalid)
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
            if (mType != DocObjectType.ObjectType)
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

        private string ParseSummary(DocContext context, string xmlText, XmlDocument xmlDoc = null)
        {
            xmlText = xmlText.Trim();
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
                                xmlText = xmlText.Remove(nodeStart, len).Insert(nodeStart, nText);

                                
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

            // Add anchor tags to URLs
            Match mUrl = _regexHttp.Match(summary);
            while (mUrl.Success)
            {
                string anchor = $"<a target=\"_blank\" href=\"{mUrl.Value}\">{mUrl.Value}</a>";
                summary = summary.Replace(mUrl.Value, anchor);
                mUrl = _regexHttp.Match(summary, mUrl.Index + anchor.Length);
            }

            return summary;
        }

        private string ParseSummaryXml(DocContext context, string xml, XmlDocument xmlDoc)
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
                        DocElement refObj = ParseXmlName(context, attCRef.Value, out DocObjectType mType, out string mName);
                        if (refObj != null) { 
                            summary = $"<a class=\"{context.Css.Target}\" data-target=\"{refObj.Namespace}\" data-target-sec=\"{refObj.Name}\">{refObj.Name}</a>";
                                }
                        else if (mType == DocObjectType.Invalid)
                            summary = $"<b class=\"{context.Css.Invalid}\" title=\"Invalid object name\">{mName}</b>";
                    }
                    break;

                case "para":
                    summary = "<br/><br/>";
                    break;
            }

            return summary;
        }
    }
}
