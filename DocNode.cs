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

        public DocObject Object { get; set; }

        public DocNode Parent { get; private set; }

        public DocNode AddChild(string name)
        {
            DocNode child = new DocNode()
            {
                Name = name,
                Parent = this,
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
