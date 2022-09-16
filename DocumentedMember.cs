using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocumentedType
    {
        [JsonProperty("member")]
        public List<DocumentedMember> Members { get; set; } = new List<DocumentedMember>();
    }

    public class DocumentedMember
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("summary")]
        public dynamic Summary { get; set; }
    }

    public class DocumentedSummary
    {
        [JsonProperty("see")]
        public dynamic See { get; set; }

        [JsonProperty("text")]
        public dynamic[] Text { get; set; }
    }
}
