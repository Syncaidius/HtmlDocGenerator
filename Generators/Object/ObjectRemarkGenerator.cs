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
    public class ObjectRemarkGenerator : ObjectSectionGenerator
    {
        public override string GetTitle()
        {
            return "Remarks";
        }

        protected override string OnGenerate(HtmlContext config, string ns, DocObject obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Remark))
                return "";
            else
                return $"<td colspan=\"3\">{obj.Remark}</td>";
        }
    }
}
