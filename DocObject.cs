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
                HtmlName = _name.Replace("<", "&lt;").Replace(">", "&gt;");
            }
        }

        public string HtmlName { get; private set; }

        public Dictionary<string, DocObject> Members { get; } = new Dictionary<string, DocObject>();

        public DocObjectType Type { get; set; }

        public Type UnderlyingType { get; set; }

        public DocObject Parent { get; set; }

        public DocData ParentDoc { get; protected set; }
    }

    public enum DocObjectType
    {
        None = 0,
         
        /// <summary>
        /// A valid type, it's unknown whether it is a class, struct or enum.
        /// </summary>
        UnspecifiedType = 1,

        Class = 2,

        Struct = 3,

        Enum = 4,

        Event = 5,

        Field = 6,

        Property = 7,

        Method = 8,
    }
}
