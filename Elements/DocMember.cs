using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocMember : DocElement
    {
        public MemberInfo BaseInfo { get; protected set; }

        public DocMember(MemberInfo info) : 
            base(info.Name)
        {
            BaseInfo = info;
        }

        public virtual bool IsMatch(DocObject obj, string name, Type[] parameters = null, Type[] genericParameters = null)
        {
            return BaseInfo.Name == name;
        }

        public MemberTypes Type => BaseInfo.MemberType;
    }
}
