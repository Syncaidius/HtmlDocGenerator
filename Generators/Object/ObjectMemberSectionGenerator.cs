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
                string contentHtml = "";

                foreach (T member in members)
                {
                    string memHtml = GetMemberHtml(ns, obj, member, false);
                    if (memHtml.Length == 0)
                        continue;

                    string iconHtml = config.GetIcon(member, "../");

                    Html(ref contentHtml, "<tr>");
                    Html(ref contentHtml, $"   <td>{iconHtml}</td>");
                    Html(ref contentHtml, $"   <td>{memHtml}</td>");

                    string mSummary = "&nbsp;";
                    if (obj.Members.TryGetValue(member.Name, out DocObject memObj))
                        mSummary = memObj.Summary;

                    Html(ref contentHtml, $"<td>{mSummary}</td>");
                    Html(ref contentHtml, $"</tr>");
                }

                if (contentHtml.Length > 0)
                {
                    Html(ref html, "<table><thead><tr>");
                    Html(ref html, $"   <th class=\"obj-section-icon\">&nbsp;</th>");
                    Html(ref html, $"   <th class=\"obj-section-title\">{GetTitle()}</th>");
                    Html(ref html, $"   <th class=\"obj-section-desc\">&nbsp</th>");
                    Html(ref html, "</tr></thead><tbody>");
                    Html(ref html, contentHtml);
                    Html(ref html, "</tbody></table><br/>");
                }
            }

            return html;
        }

        public override sealed string GenerateIndexTreeItems(HtmlContext config, string ns, DocObject obj)
        {
            IEnumerable<T> members = obj.TypeMembers.Where(x => (x as T) != null).Cast<T>();
            if (members.Count() == 0)
                return "";

            string html = "";
            string contentHtml = "";

            foreach (T member in members)
            {
                string memHtmlName = HtmlHelper.GetHtmlName(member.Name);
                if (memHtmlName.Length == 0)
                    continue;

                string nsMember = $"{ns}-{memHtmlName}";
                string memberHtml = GetMemberHtml(nsMember, obj, member, true);

                if (memberHtml.Length == 0)
                    continue;

                string htmlIcon = config.GetIcon(member);

                contentHtml += $"   <tr id=\"{nsMember}\" class=\"sec-namespace-obj\">{Environment.NewLine}";
                contentHtml += $"       <td>{htmlIcon}</td>{Environment.NewLine}";
                contentHtml += $"       <td><span class=\"doc-page-target\" data-url=\"{obj.PageUrl}\">{memberHtml}</span></td>{Environment.NewLine}";
                contentHtml += $"    </tr>{Environment.NewLine}";
            }

            if (contentHtml.Length > 0)
            {
                Html(ref html, $"<table class=\"sec-obj-index\"><thead><tr>");
                Html(ref html, $"<th class=\"col-type-icon\"></th>");
                Html(ref html, $"<th class=\"col-type-name\"></th>");
                Html(ref html, $"</tr></thead><tbody>");
                Html(ref html, contentHtml);
                Html(ref html, $"</tbody></table>");
            }

            return html;
        }        

        protected virtual string GetMemberHtml(string ns, DocObject obj, T member, bool isIndex)
        {
            return HtmlHelper.GetHtmlName(member.Name);
        }
    }
}
