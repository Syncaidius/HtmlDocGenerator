using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public abstract class HtmlGenerator
    {

        protected void Html(ref string html, string newHtml)
        {
            html += $"{newHtml}{Environment.NewLine}";
        }
    }
}
