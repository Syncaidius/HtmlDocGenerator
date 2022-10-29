using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DocObject : DocElement
    {
        Type _type;

        public DocObject(string name) : base(name, DocObjectType.ObjectType) { }

        public DocMember GetMember<T>(string name, Type[] parameters = null, Type[] genericParameters = null)
            where T : DocMember
        {
            if (Members.TryGetValue(name, out List<DocElement> memList))
            {
                foreach (DocElement element in memList)
                {
                    if (element is DocMember member)
                    {
                        if (member.IsMatch(this, name, parameters, genericParameters))
                            return member;
                    }
                }
            }

            return null;
        }

        private void BuildTypeInfo()
        {
            if (_type == null)
            {
                DocType = DocObjectType.Unknown;
                Members.Clear();
            }
            else
            {
                if(_type.BaseType != null)
                    BaseTypeName = $"{_type.BaseType.Namespace}.{HtmlHelper.GetHtmlName(_type.BaseType)}";

                MemberInfo[] members = _type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                foreach(MemberInfo member in members)
                {
                    DocMember dm = null;
                    switch (member)
                    {
                        case MethodInfo mb:
                            if (!mb.IsSpecialName)
                                dm = new DocMethodMember(this, mb);
                            break;

                        case ConstructorInfo ci:
                                dm = new DocMethodMember(this, ci);
                            break;

                        case PropertyInfo pi:
                            dm = new DocPropertyMember(this, pi);
                            break;

                        default:
                            dm = new DocMember(this, member);
                            break;
                    }

                    if (dm == null)
                        continue;

                    AddMember(dm);
                }

                // Figure out the type of object that is defined.
                if (_type.IsClass)
                {
                    DocType = DocObjectType.Class;
                }
                else if (_type.IsInterface)
                {
                    DocType = DocObjectType.Interface;
                }
                else if (_type.IsValueType)
                {
                    if (_type.IsEnum)
                        DocType = DocObjectType.Enum;
                    else
                        DocType = DocObjectType.Struct;
                }
            }
        }

        private void AddMember(DocMember member)
        {
            if (!Members.TryGetValue(member.Name, out List<DocElement> memList))
            {
                memList = new List<DocElement>();
                Members.Add(member.Name, memList);
            }

            memList.Add(member);
        }

        public override string ToString()
        {
            return $"{Name} - {DocType} - Members: {Members.Count}";
        }

        [JsonProperty]
        public string BaseTypeName { get; private set; }

        public Type UnderlyingType
        {
            get => _type;
            set
            {
                if(_type != value)
                {
                    _type = value;
                    BuildTypeInfo();
                }
            }
        }

        public DocObject Parent { get; set; }

        public DocAssembly Assembly { get; set; }

        public override string Namespace => _type.Namespace;
    }
}
