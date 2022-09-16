using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class Documentation
    {
        [JsonProperty("members")]
        public DocumentedType Members { get; set; }

        [JsonProperty("assembly")]
        public DocumentationAssembly Assembly { get; set; }
    }

    public class DocumentationAssembly
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
