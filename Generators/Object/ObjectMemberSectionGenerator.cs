//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Metrics;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace HtmlDocGenerator
//{
//    public abstract class ObjectMemberSectionGenerator : ObjectSectionGenerator
//    {
//        public abstract string GenerateIndexTreeItems(HtmlContext config, string ns, DocObject obj);
//    }

//    /// <summary>
//    /// Base class for generators which output html for a member-list section of an object page.
//    /// </summary>
//    public abstract class ObjectMemberSectionGenerator<T> : ObjectMemberSectionGenerator
//        where T : MemberInfo
//    {
//        protected override string OnGenerate(HtmlContext config, string ns, DocObject obj)
//        {
//            string html = "";
//            if (!obj.MembersByType.TryGetValue(MemberType, out List<DocMember> members))
//                return html;

//            if (members.Count == 0)
//                return "";

//            foreach (DocMember dm in members)
//            {
//                T member = dm.BaseInfo as T;
//                string memHtml = GetMemberHtml(ns, obj, member, false);
//                if (memHtml.Length == 0)
//                    continue;

//                string iconHtml = config.GetIcon(member, "../");

//                Html(ref html, "<tr>");
//                Html(ref html, $"   <td>{iconHtml}</td>");
//                Html(ref html, $"   <td>{memHtml}</td>");
//                Html(ref html, $"   <td>{dm.Summary}</td>");
//                Html(ref html, $"</tr>");
//            }

//            return html;
//        }

//        public override sealed string GenerateIndexTreeItems(HtmlContext config, string ns, DocObject obj)
//        {
//            string html = "";

//            if (!obj.MembersByType.TryGetValue(MemberType, out List<DocMember> memList))
//                return html;

//            if (memList.Count == 0)
//                return "";

//            string contentHtml = "";

//            DocMember prev = null;

//            foreach (DocMember m in memList)
//            {
//                if (prev?.BaseInfo.Name == m.BaseInfo.Name)
//                    continue;

//                T info = m.BaseInfo as T;

//                string memHtmlName = HtmlHelper.GetHtml(info.Name);
//                if (memHtmlName.Length == 0)
//                    continue;

//                string nsMember = $"{ns}-{memHtmlName}";
//                string memberHtml = GetMemberHtml(nsMember, obj, info, true);

//                if (memberHtml.Length == 0)
//                    continue;

//                string htmlIcon = config.GetIcon(info);

//                contentHtml += $"   <tr id=\"{nsMember}\" class=\"sec-namespace-obj\">{Environment.NewLine}";
//                contentHtml += $"       <td>{htmlIcon}</td>{Environment.NewLine}";
//                contentHtml += $"       <td><span class=\"doc-page-target\" data-url=\"{obj.HtmlUrl}\">{memberHtml}</span></td>{Environment.NewLine}";
//                contentHtml += $"    </tr>{Environment.NewLine}";

//                prev = m;
//            }

//            if (contentHtml.Length > 0)
//            {
//                Html(ref html, $"<table class=\"sec-obj-index\"><thead><tr>");
//                Html(ref html, $"<th class=\"col-type-icon\"></th>");
//                Html(ref html, $"<th class=\"col-type-name\"></th>");
//                Html(ref html, $"</tr></thead><tbody>");
//                Html(ref html, contentHtml);
//                Html(ref html, $"</tbody></table>");
//            }

//            return html;
//        }        

//        protected virtual string GetMemberHtml(string ns, DocObject obj, T member, bool isIndex)
//        {
//            return HtmlHelper.GetHtml(member.Name);
//        }

//        public abstract MemberTypes MemberType { get; }
//    }
//}
