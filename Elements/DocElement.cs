using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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

        public DocElement(string name, DocObjectType initialType)
        {
            Name = name;
            ObjectType = initialType;
        }

        [JsonProperty]
        public string Summary { get; set; }

        [JsonProperty]
        public string Remark { get; set; }

        public abstract string Namespace { get; }

        [JsonProperty]
        public DocObjectType ObjectType { get; set; }

        public void AddMember(DocElement element)
        {
            if (!Members.TryGetValue(element.Name, out List<DocElement> memList))
            {
                memList = new List<DocElement>();
                Members.Add(element.Name, memList);
            }

            memList.Add(element);
        }

        
        public Dictionary<string, List<DocElement>> Members { get; set; } = new Dictionary<string, List<DocElement>>();

        /// <summary>
        /// A null-if-zero-count version of <see cref="Members"/>. This is used for simplifying serialization.
        /// </summary>
        [JsonProperty("Members")]
        public object Elements => Members.Count > 0 ? Members : null;

        public virtual string Name { get; set; }
    }
}
