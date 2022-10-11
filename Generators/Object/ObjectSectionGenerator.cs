using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    /// <summary>
    /// Base class for generators which output html for a section of an object page.
    /// </summary>
    public abstract class ObjectSectionGenerator : HtmlGenerator
    {
        public string Generate(DocObject obj)
        {
            string html = "";
            Html(ref html, "<div class=\"obj-section\">");
            Html(ref html, OnGenerate(obj));
            Html(ref html, "</div>");

            return html;
        }

        public abstract string GetTitle();

        protected abstract string OnGenerate(DocObject obj);

        public abstract string GenerateIndexTreeItems(string ns, DocObject obj);
    }
}
