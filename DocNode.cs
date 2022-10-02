using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocNode
    {
        public string Name { get; set; }

        public DocNodeType Type { get; set; }

        public List<DocNode> Children { get; } = new List<DocNode>();

        public DocNode AddChild<T>(string name)
        {
            DocNode child = new DocNode()
            {
                Name = name,
            };

            Children.Add(child);
            return child;
        }
    }

    public enum DocNodeType
    {
        Text = 0,

        Document = 1,

        AssemblyName = 2,
    }
}
