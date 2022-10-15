using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HtmlDocGenerator
{
    public class DocParser
    {
        Dictionary<char, DocObjectType> _typeKeys = new Dictionary<char, DocObjectType>()
        {
            ['T'] = DocObjectType.ObjectType,
            ['E'] = DocObjectType.Event,
            ['P'] = DocObjectType.Property,
            ['F'] = DocObjectType.Field,
            ['M'] = DocObjectType.Method
        };

        /// <summary>
        /// Parses a summary xml file and adds it's information to the provided <see cref="HtmlContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="HtmlContext"/> to which the parsed XML will be added.</param>
        /// <param name="xmlPath">The path of the XML summary file to be parsed.</param>
        /// <param name="assemblyPath">A custom path to the assembly file from which the xml summary file was generated.</param>
        public void ParseXml(HtmlContext context, string xmlPath, string assemblyPath = null)
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

                    XmlNode docNode = xml["doc"];
                    if (docNode != null)
                    {
                        DocAssembly docAssembly = ParseAssembly(context, docNode, info, assemblyPath);
                        if (docAssembly != null)
                            ParseMembers(context, docNode, docAssembly);
                    }
                    else
                    {
                        context.Error($"The root 'doc' node was not found. Unable to continue.");
                    }
                }
            }
        }

        private DocAssembly ParseAssembly(HtmlContext context, XmlNode docRoot, FileInfo xmlInfo, string assemblyPath)
        {
            XmlNode xAssembly = docRoot["assembly"];
            DocAssembly da = null;

            if (xAssembly != null)
            {
                if (!context.Assemblies.TryGetValue(xAssembly.InnerText, out da))
                {
                    da = new DocAssembly();
                    da.Name = xAssembly.InnerText;
                    da.FilePath = assemblyPath;

                    if (string.IsNullOrWhiteSpace(da.FilePath))
                        da.FilePath = $"{xmlInfo.Directory}\\{da.Name}.dll";

                    if (!da.Load(context))
                        return null;
                        
                    context.Assemblies.Add(da.Name, da);

                    // Run through the public types in the assembly.
                    try
                    {
                        context.Log($"Retrieving type list for '{da.Name}'");
                        Type[] aTypes = da.Assembly.GetExportedTypes();
                        context.Log($"Retrieved {aTypes.Length} public types for '{da.Name}'");

                        foreach (Type t in aTypes)
                        {
                            DocObject obj = ParseQualifiedName(context, t.FullName, DocObjectType.ObjectType);
                            obj.UnderlyingType = t;

                            if (t.IsGenericType)
                                obj.Name = HtmlHelper.GetHtmlName(t);
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Error($"Failed to retrieve public types from '{da.Name}': {ex.Message}");
                    }
                }
            }

            return da;
        }

        private void ParseMembers(HtmlContext context, XmlNode docRoot, DocAssembly assembly)
        {
            XmlNode xMembers = docRoot["members"];
            if(xMembers != null)
            {
                foreach(XmlNode node in xMembers.ChildNodes)
                    ParseMember(context, node);
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

            DocObjectType mType = DocObjectType.None;
            string typeName = attName.Value;
            char startsWith = typeName[0];
            if (!_typeKeys.TryGetValue(startsWith, out mType))
            {
                context.Error($"Invalid type-name prefix '{startsWith}' for '{typeName}'");
                return;
            }

            typeName = typeName.Substring(2); // Get typeName without the [type]: prefix

            ParseQualifiedName(context, typeName, mType);
        }

        private DocObject ParseQualifiedName(HtmlContext context, string typeName, DocObjectType mType)
        {
            string ns = "";
            string nsPrev = "";

            string objectName = "";
            string memberName = "";

            string prev = "";
            string cur = "";

            List<Type> methodParams = new List<Type>();
            DocObject memberObj = null;

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

            if(mType != DocObjectType.ObjectType)
            {
                DocObject parentObj = context.GetObject(ns, memberName);

                if (mType != DocObjectType.Method)
                    parentObj.AddMember<DocMember>(memberName, mType);
                else
                    parentObj.AddMember<DocMethodMember>(memberName, mType, methodParams.ToArray());

                // TODO Add support for multiple members of the same name (dictionary of List<DocMember>).
            }
            else
            {
                memberObj = context.GetObject(ns, memberName);
            }

            return memberObj;
        }
    }
}
