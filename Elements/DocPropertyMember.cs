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
    public class DocPropertyMember : DocMember
    {
        public DocPropertyMember(DocObject parent, PropertyInfo info) : base(parent, info)
        {
            Info = info;
            TypeName = $"{Namespace}.{HtmlHelper.GetHtmlName(Info.PropertyType)}";

            if (Info.GetMethod != null)
                Getter = new DocMethodMember(parent, Info.GetMethod);

            if (Info.SetMethod != null)
                Setter = new DocMethodMember(parent, Info.SetMethod);
        }

        public PropertyInfo Info { get; }

        public string TypeName { get; }

        [JsonProperty]
        public DocMethodMember Getter { get; }

        [JsonProperty]
        public DocMethodMember Setter { get; }

        public override string Namespace => Info.PropertyType.Namespace;
    }
}
