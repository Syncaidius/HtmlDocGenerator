using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocObject : DocElement
    {
        Type _type;

        public DocObject(string name) : base(name)
        {
            MembersByType = new Dictionary<MemberTypes, List<DocMember>>();
        }

        public DocMember GetMember<T>(string name, Type[] parameters = null, Type[] genericParameters = null)
            where T : DocMember
        {
            if (MembersByName.TryGetValue(name, out List<DocMember> memList))
            {
                foreach (DocMember member in memList)
                {
                    if (member.IsMatch(this, name, parameters, genericParameters))
                        return member;
                }
            }

            return null;
        }

        private void BuildTypeInfo()
        {
            if (_type == null)
            {
                DocType = DocObjectType.Unknown;
                MembersByName.Clear();
            }
            else
            {
                MemberInfo[] members = _type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                foreach(MemberInfo member in members)
                {
                    DocMember dm = null;
                    switch (member)
                    {
                        case MethodInfo mb:
                            if (!mb.IsSpecialName)
                                dm = new DocMethodMember(mb);
                            break;

                        case ConstructorInfo ci:
                                dm = new DocMethodMember(ci);
                            break;

                        default:
                            dm = new DocMember(member);
                            break;
                    }

                    if (dm == null)
                        continue;

                    dm.Namespace = Namespace;

                    if(!MembersByName.TryGetValue(member.Name, out List<DocMember> memList))
                    {
                        memList = new List<DocMember>();
                        MembersByName.Add(member.Name, memList);
                    }

                    if (!MembersByType.TryGetValue(member.MemberType, out List<DocMember> byTypeList))
                    {
                        byTypeList = new List<DocMember>();
                        MembersByType.Add(member.MemberType, byTypeList);
                    }

                    memList.Add(dm);
                    byTypeList.Add(dm);
                }

                // Sort member lists
                foreach (List<DocMember> objList in MembersByType.Values)
                    objList.OrderBy(o => o.Name);

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

        public override string ToString()
        {
            return $"{Name} - {XmlType} - Members: {MembersByName.Count}";
        }

        public Dictionary<string, List<DocMember>> MembersByName { get; } = new Dictionary<string, List<DocMember>>();

        public Dictionary<MemberTypes, List<DocMember>> MembersByType { get; } = new Dictionary<MemberTypes, List<DocMember>>();

        public XmlMemberType XmlType { get; set; }

        public DocObjectType DocType { get; set; }

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
    }

    public enum XmlMemberType
    {
        None = 0,
         
        /// <summary>
        /// A valid object type.
        /// </summary>
        ObjectType = 1,

        Event = 5,

        Field = 6,

        Property = 7,

        Method = 8,

        Invalid = 16,
    }

    public enum DocObjectType
    {
        Unknown = 0,
         
        Class = 1,

        Struct = 2,

        Enum = 3,

        Interface = 4,
    }
}
