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

            if (TypeName.EndsWith('&'))
            {
                TypeName = TypeName.Substring(0, TypeName.Length - 1);

                if (Info.IsOut)
                    Keyword = DocParameterKeyword.Out;
                else if (Info.IsIn)
                    Keyword = DocParameterKeyword.In;
                else
                    Keyword = DocParameterKeyword.Ref;
            }
        }

        public ParameterInfo Info { get; }

        [JsonProperty]
        public string TypeName { get; }

        [JsonProperty]
        public DocParameterKeyword? Keyword { get; }

        public override string Namespace => Info.ParameterType.Namespace;

        [JsonProperty]
        public override string Name
        {
            get => base.Name;
            set => base.Name = value;
        }
    }

    public enum DocParameterKeyword
    {
        None = 0,

        In = 1,

        Out = 2,

        Ref = 3,
    }
}
