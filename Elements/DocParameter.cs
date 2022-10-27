using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HtmlDocGenerator
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DocParameter : DocElement
    {
        public DocParameter(ParameterInfo info) : base(info.Name, DocObjectType.Parameter)
        {
            Info = info;
        }

        public ParameterInfo Info { get; }

        public override string Namespace => Info.ParameterType.Namespace;
    }
}
