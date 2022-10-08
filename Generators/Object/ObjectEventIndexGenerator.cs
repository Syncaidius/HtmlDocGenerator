using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class ObjectEventIndexGenerator : ObjectSectionGenerator
    {
        protected override string OnGenerate(DocObject obj)
        {
            string html = "";
            Html(ref html, "<table><thead><tr>");
            Html(ref html, $"   <th class=\"obj-section-title\">Events</th>");
            Html(ref html, $"   <th>&nbsp</th>");
            Html(ref html, "</tr></thead><tbody>");

            foreach (MemberInfo mInfo in obj.TypeMembers)
            {
                if (mInfo.MemberType != MemberTypes.Event)
                    continue;

                Html(ref html, "<tr>");
                Html(ref html, $"   <td>{mInfo.Name}</td>");

                string mSummary = "&nbsp;";
                if (obj.Members.TryGetValue(mInfo.Name, out DocObject memObj))
                    mSummary = memObj.Summary;

                Html(ref html, $"<td>{mSummary}</td>");
                Html(ref html, $"</tr>");
            }

            html += "</tbody></table>";

            return html;
        }
    }
}
