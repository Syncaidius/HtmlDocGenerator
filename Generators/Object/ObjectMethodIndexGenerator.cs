using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class ObjectMethodIndexGenerator : ObjectMemberSectionGenerator<MethodInfo>
    {
        protected override string OnGenerateMemberSection(DocObject obj, IEnumerable<MethodInfo> members)
        {
            string html = "";
            foreach (MethodInfo info in members)
            {
                if (info.IsSpecialName)
                    continue;

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
            return "Methods";
        }
    }
}
