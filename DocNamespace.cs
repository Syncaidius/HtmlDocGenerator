using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DocNamespace : DocElement
    {
        public override string Namespace => Name;

        public DocNamespace(string name) : base(name, DocObjectType.Namespace)
        {
            Name = name;
        }
    }
}
