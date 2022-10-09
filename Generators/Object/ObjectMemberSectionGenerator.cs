using System;
using System.Collections.Generic;
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
        protected override string OnGenerate(DocObject obj)
        {
            IEnumerable<T> mInfo = obj.TypeMembers.Where(x => (x as T) != null).Cast<T>();
           
            string html = "";

            if (mInfo.Count() > 0)
            {
                mInfo = mInfo.OrderBy(x => x.Name);

                Html(ref html, "<table><thead><tr>");
                Html(ref html, $"   <th class=\"obj-section-title\">{GetTitle()}</th>");
                Html(ref html, $"   <th class=\"obj-section-desc\">&nbsp</th>");
                Html(ref html, "</tr></thead><tbody>");

                html += OnGenerateMemberSection(obj, mInfo);
                html += "</tbody></table><br/>";
            }

            return html;
        }

        protected abstract string GetTitle();

        protected abstract string OnGenerateMemberSection(DocObject obj, IEnumerable<T> members);
    }
}
