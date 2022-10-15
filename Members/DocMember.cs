using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocMember
    {
        public MemberInfo Info { get; protected set; }

        public string Summary { get; set; }

        public virtual void Set(DocObject obj, string name, Type[] parameters = null, Type[] genericParameters = null)
        {
            Info = obj.UnderlyingType.GetMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault();
        }

        public virtual bool IsMatch(DocObject obj, string name, Type[] parameters = null, Type[] genericParameters = null)
        {
            return Info.Name == name;
        }

        public DocObjectType Type { get; set; }
    }
}
