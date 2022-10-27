﻿using Newtonsoft.Json;
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

        public DocObject(string name) : base(name, DocObjectType.ObjectType)
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
                                dm = new DocMethodMember(this, mb);
                            break;

                        case ConstructorInfo ci:
                                dm = new DocMethodMember(this, ci);
                            break;

                        default:
                            dm = new DocMember(this, member);
                            break;
                    }

                    if (dm == null)
                        continue;

                    AddMember(dm);
                }

                // Sort member lists
                NameComparer nameComparer = new NameComparer();
                foreach (List<DocMember> objList in MembersByType.Values)
                    objList.Sort(nameComparer);

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
            if (!MembersByName.TryGetValue(member.Name, out List<DocMember> memList))
            {
                memList = new List<DocMember>();
                MembersByName.Add(member.Name, memList);
            }

            if (!MembersByType.TryGetValue(member.BaseInfo.MemberType, out List<DocMember> byTypeList))
            {
                byTypeList = new List<DocMember>();
                MembersByType.Add(member.BaseInfo.MemberType, byTypeList);
            }

            memList.Add(member);
            byTypeList.Add(member);
        }

        public override string ToString()
        {
            return $"{Name} - {DocType} - Members: {MembersByName.Count}";
        }

        public Dictionary<string, List<DocMember>> MembersByName { get; } = new Dictionary<string, List<DocMember>>();
        
        public Dictionary<MemberTypes, List<DocMember>> MembersByType { get; } = new Dictionary<MemberTypes, List<DocMember>>();

        [JsonProperty]
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

        public override string Namespace => _type.Namespace;
    }
}
