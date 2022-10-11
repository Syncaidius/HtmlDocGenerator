using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    /// <summary>
    /// Base class for generators which output html for a member-list section of an object page.
    /// </summary>
    public abstract class ObjectMemberSectionGenerator<T> : ObjectSectionGenerator
        where T : MemberInfo
    {
        protected override string OnGenerate(HtmlContext config, string ns, DocObject obj)
        {
            IEnumerable<T> members = obj.TypeMembers.Where(x => (x as T) != null).Cast<T>();
           
            string html = "";

            if (members.Count() > 0)
            {
                members = members.OrderBy(x => x.Name);

                Html(ref html, "<table><thead><tr>");
                Html(ref html, $"   <th class=\"obj-section-icon\">&nbsp;</th>");
                Html(ref html, $"   <th class=\"obj-section-title\">{GetTitle()}</th>");
                Html(ref html, $"   <th class=\"obj-section-desc\">&nbsp</th>");
                Html(ref html, "</tr></thead><tbody>");

                foreach (T member in members)
                {
                    string iconHtml = config.GetIcon(member);
                    string memHeml = GetMemberHtml(ns, obj, member, false);

                    Html(ref html, "<tr>");
                    Html(ref html, $"   <td>{iconHtml}</td>");
                    Html(ref html, $"   <td>{memHeml}</td>");

                    string mSummary = "&nbsp;";
                    if (obj.Members.TryGetValue(member.Name, out DocObject memObj))
                        mSummary = memObj.Summary;

                    Html(ref html, $"<td>{mSummary}</td>");
                    Html(ref html, $"</tr>");
                }

                html += "</tbody></table><br/>";
            }

            return html;
        }

        public override sealed string GenerateIndexTreeItems(HtmlContext config, string ns, DocObject obj)
        {
            IEnumerable<T> members = obj.TypeMembers.Where(x => (x as T) != null).Cast<T>();
            if (members.Count() == 0)
                return "";

            string nl = Environment.NewLine;
            string html = $"<table class=\"sec-obj-index\"><thead><tr>{nl}";

            html += $"<th class=\"col-type-icon\"></th>{nl}";
            html += $"<th class=\"col-type-name\"></th>{nl}";
            html += $"</tr></thead><tbody>{nl}";
            foreach(T member in members)
            {
                string memHtmlName = HtmlHelper.GetHtmlName(member.Name);
                if (memHtmlName.Length == 0)
                    continue;

                string nsMember = $"{ns}-{memHtmlName}";
                string memberHtml = GetMemberHtml(nsMember, obj, member, true);

                if (memberHtml.Length == 0)
                    continue;

                string htmlIcon = config.GetIcon(member);

                html += $"   <tr id=\"{nsMember}\" class=\"sec-namespace-obj\">{Environment.NewLine}";
                html += $"       <td>{htmlIcon}</td>{Environment.NewLine}";
                html += $"       <td><span class=\"doc-page-target\" data-url=\"{obj.PageUrl}\">{memberHtml}</span></td>{Environment.NewLine}";
                html += $"    </tr>{Environment.NewLine}";
            }
            html += $"</tbody></table>{nl}";

            return html;
        }        

        protected virtual string GetMemberHtml(string ns, DocObject obj, T member, bool isIndex)
        {
            return HtmlHelper.GetHtmlName(member.Name);
        }
    }
}
