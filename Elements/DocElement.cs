using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HtmlDocGenerator
{
    public abstract class DocElement
    {
        string _name;

        public DocElement(string name)
        {
            Name = name;
        }

        public string Summary { get; set; } = "&nbsp;";

        /// <summary>
        /// Gets the Url to the page containing information about the current <see cref="DocElement"/>.
        /// </summary>
        public string PageUrl { get; set; }

        public string HtmlName { get; private set; }

        public abstract string Namespace { get; }

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
