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

                string paramString = "";
                ParameterInfo[] parameters = info.GetParameters();
                for(int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo pi = parameters[i];
                    if (i > 0)
                        paramString += ", ";

                    paramString += HtmlHelper.GetHtmlName(pi.ParameterType);
                }

                Html(ref html, "<tr>");
                Html(ref html, $"   <td>{info.Name}({paramString})</td>");

                string mSummary = "&nbsp;";
                if (obj.Members.TryGetValue(info.Name, out DocObject memObj))
                    mSummary = memObj.Summary;

                Html(ref html, $"<td>{mSummary}</td>");
                Html(ref html, $"</tr>");
            }

            return html;
        }

        public override string GetTitle()
        {
            return "Methods";
        }

        protected override string GetMemberHtml(string ns, DocObject obj, string memberHtmlName, MethodInfo member)
        {
            if (member.IsSpecialName)
                return "";
            else 
                return $"       <td><span class=\"doc-page-target\" data-url=\"{obj.PageUrl}\">{memberHtmlName}</span></td>{Environment.NewLine}";
        }
    }
}
