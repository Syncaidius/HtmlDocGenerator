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
        [JsonProperty]
        public List<DocObject> Objects { get; } = new List<DocObject>();

        public DirectoryInfo DestDirectory { get; set; }

        public override string Namespace => Name;

        public DocNamespace(string name) : base(name)
        {
            Name = name;
        }
    }
}
