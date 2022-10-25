using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DocNamespace
    {
        [JsonProperty]
        public string Name { get;}

        [JsonProperty]
        public List<DocObject> Objects { get; } = new List<DocObject>();

        public DirectoryInfo DestDirectory { get; set; }

        public DocNamespace(string name)
        {
            Name = name;
        }
    }
}
