using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    [JsonObject]
    public class DocData
    {
        public string Title { get; set; }

        public Dictionary<string, DocNamespace> Namespaces { get; set; }
    }
}
