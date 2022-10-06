using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocData : DocObject
    {
        public DocData() : base(null, "doc", DocObjectType.None)
        {
            ParentDoc = this;
        }

        public string AssemblyName { get; set; }

        public Assembly Assembly { get; set; }
    }
}
