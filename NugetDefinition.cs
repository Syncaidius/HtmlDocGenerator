using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class NugetDefinition
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string Framework { get; set; }

        public NugetDefinition()
        {
            Name = "";
            Version = "";
            Framework = "";
        }

        public NugetDefinition(string name, string version, string framework)
        {
            Name = name;
            Version = version;
            Framework = framework;
        }
    }
}
