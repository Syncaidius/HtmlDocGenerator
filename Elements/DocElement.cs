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
    public abstract class DocElement
    {
        public class NameComparer : IComparer<DocElement>
        {
            public int Compare(DocElement x, DocElement y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }

        string _name;

        public DocElement(string name)
        {
            Name = name;
        }

        [JsonProperty]
        public string Summary { get; set; }

        [JsonProperty]
        public string Remark { get; set; }

        /// <summary>
        /// Gets the Url to the page containing information about the current <see cref="DocElement"/>.
        /// </summary>
        public string HtmlUrl { get; set; }

        public string PageFilePath { get; set; }

        public string HtmlName { get; private set; }

        public abstract string Namespace { get; }

        [JsonProperty]
        public Dictionary<string, List<DocElement>> Members { get; set; } = new Dictionary<string, List<DocElement>>();

        [JsonProperty]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                HtmlName = HtmlHelper.GetHtml(_name);
            }
        }
    }
}
