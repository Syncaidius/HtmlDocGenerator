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
    public class DocMember : DocElement
    {
        public MemberInfo BaseInfo { get; protected set; }

        public DocMember(DocObject parent, MemberInfo info) : 
            base(info.Name, DocObjectType.Unknown)
        {
            Parent = parent;
            BaseInfo = info;
            DeclaringType = BaseInfo.DeclaringType.FullName; // TODO parse name incase it's generic.

            switch (BaseInfo.MemberType)
            {
                case MemberTypes.Property: ObjectType = DocObjectType.Property; break;
                case MemberTypes.Field: ObjectType = DocObjectType.Field; break;
                case MemberTypes.Method: ObjectType = DocObjectType.Method; break;
                case MemberTypes.Constructor: ObjectType = DocObjectType.Constructor;break;
                case MemberTypes.Event: ObjectType = DocObjectType.Event; break;
            }
        }

        public virtual bool IsMatch(DocObject obj, string name, Type[] parameters = null, Type[] genericParameters = null)
        {
            return BaseInfo.Name == name;
        }

        public override string ToString()
        {
            return $"{BaseInfo.Name} - Type: {BaseInfo.MemberType}";
        }

        [JsonProperty]
        public string DeclaringType { get; }

        public DocObject Parent { get; }

        public override string Namespace => Parent.Namespace;
    }
}
