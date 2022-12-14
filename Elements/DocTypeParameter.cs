using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HtmlDocGenerator
{
    public class DocTypeParameter : DocElement
    {
        public DocTypeParameter(Type type) : base(type.Name, DocObjectType.TypeParameter)
        {
            ParameterType = type;
        }

        public Type ParameterType { get; }

        public override string Namespace => ParameterType.Namespace;
    }
}
