using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocData : DocObject
    {
        public DocData() : base("doc", DocObjectType.None) { }

        public DocAssembly Assembly { get; set; }
    }

    public class DocAssembly
    {
        public string Name { get; set; }

        public DocAssembly(string name)
        {
            Name = name;
        }
    }
}
