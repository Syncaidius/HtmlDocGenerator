using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocNamespace
    {
        public string Name { get;}

        public List<DocObject> Objects { get; } = new List<DocObject>();

        public DirectoryInfo DestDirectory { get; set; }

        public DocNamespace(string name)
        {
            Name = name;
        }
    }
}
