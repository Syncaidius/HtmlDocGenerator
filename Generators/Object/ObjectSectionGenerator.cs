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
        public string Generate(HtmlContext config, string ns, DocObject obj)
        {
            string html = "";
            string contentHtml = OnGenerate(config, ns, obj);

            if (!string.IsNullOrWhiteSpace(contentHtml))
            {
                Html(ref html, "<div class=\"obj-section\">");
                Html(ref html, "    <table><thead><tr>");
                Html(ref html, $"       <th class=\"obj-section-icon\">&nbsp;</th>");
                Html(ref html, $"       <th class=\"obj-section-title\">{GetTitle()}</th>");
                Html(ref html, $"       <th class=\"obj-section-desc\">&nbsp</th>");
                Html(ref html, "    </tr></thead><tbody>");
                Html(ref html, contentHtml);
                Html(ref html, "    </tbody></table><br/>");
                Html(ref html, "</div>");
            }

            return html;
        }

        public abstract string GetTitle();

        protected abstract string OnGenerate(HtmlContext config, string ns, DocObject obj);

    }
}
