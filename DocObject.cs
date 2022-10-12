using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocObject
    {
        string _name;
        Type _type;

        public DocObject(DocData parent, string name, DocObjectType type)
        {
            ParentDoc = parent;
            Name = name;
            Type = type;
        }

        public DocObject AddMember(string name, DocObjectType type)
        {
            DocObject member = new DocObject(ParentDoc, name, type);
            member.Parent = this;

            Members.Add(name, member);
            return member;
        }

        private void BuildTypeInfo()
        {
            if (_type == null)
            {
                SubType = DocObjectSubType.Unknown;
                TypeMembers = null;
            }
            else
            {
                TypeMembers = _type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                if (_type.IsClass)
                {
                    SubType = DocObjectSubType.Class;
                }
                else if (_type.IsInterface)
                {
                    SubType = DocObjectSubType.Interface;
                }
                else if (_type.IsValueType)
                {
                    if (_type.IsEnum)
                        SubType = DocObjectSubType.Enum;
                    else
                        SubType = DocObjectSubType.Struct;
                }
            }
        }

        public override string ToString()
        {
            return $"{Name} - {Type} - Members: {Members.Count}";
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                HtmlName = HtmlHelper.GetHtmlName(_name);
            }
        }

        public string HtmlName { get; private set; }

        public Dictionary<string, DocObject> Members { get; } = new Dictionary<string, DocObject>();

        public DocObjectType Type { get; set; }

        public DocObjectSubType SubType { get; set; }

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

        public DocData ParentDoc { get; protected set; }

        /// <summary>
        /// Gets the Url to the page containing information about the current <see cref="DocObject"/>.
        /// </summary>
        public string PageUrl { get; set; }

        public string Summary { get; set; }

        public MemberInfo[] TypeMembers { get; private set; }
    }

    public enum DocObjectType
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
    }

    public enum DocObjectSubType
    {
        Unknown = 0,
         
        Class = 1,

        Struct = 2,

        Enum = 3,

        Interface = 4,
    }
}
