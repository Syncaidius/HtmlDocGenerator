using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class ObjectEventIndexGenerator : ObjectMemberSectionGenerator<EventInfo>
    {
        protected override string OnGenerateMemberSection(DocObject obj, IEnumerable<EventInfo> members)
        {
            string html = "";

            foreach (EventInfo info in members)
            {
                Html(ref html, "<tr>");
                Html(ref html, $"   <td>{info.Name}</td>");

                string mSummary = "&nbsp;";
                if (obj.Members.TryGetValue(info.Name, out DocObject memObj))
                    mSummary = memObj.Summary;

                Html(ref html, $"<td>{mSummary}</td>");
                Html(ref html, $"</tr>");
            }

            return html;
        }

        protected override string GetTitle()
        {
            return "Events";
        }
    }
}
