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
            TypeName = $"{Info.ParameterType.Namespace}.{HtmlHelper.GetHtmlName(Info.ParameterType)}";
        }

        public ParameterInfo Info { get; }

        [JsonProperty]
        public string TypeName { get; }

        public override string Namespace => Info.ParameterType.Namespace;

        [JsonProperty]
        public override string Name
        {
            get => base.Name;
            set => base.Name = value;
        }
    }
}
