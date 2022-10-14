using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class ObjectMethodIndexGenerator : ObjectMemberSectionGenerator<MethodBase>
    {
        public override string GetTitle()
        {
            return "Methods";
        }

        protected virtual bool IsValidMethod(MethodBase member)
        {
            return !member.IsSpecialName;
        }

        protected virtual string GetMethodName(DocObject obj, MethodBase member)
        {
            return HtmlHelper.GetHtmlName(member.Name);
        }

        protected override sealed string GetMemberHtml(string ns, DocObject obj, MethodBase member, bool isIndex)
        {
            if (!IsValidMethod(member))
            {
                return "";
            }
            else
            {
                string paramHtml = "";

                if (!isIndex)
                {
                    ParameterInfo[] parameters = member.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterInfo pi = parameters[i];
                        if (i > 0)
                            paramHtml += ", ";

                        paramHtml += $"<i class=\"obj-parameter\">{HtmlHelper.GetHtmlName(pi.ParameterType)}</i>";
                    }

                    paramHtml = $"<i class=\"obj-parenthesis\">(</i>{paramHtml}<i class=\"obj-aparenthesis\">)</i>";
                }

                string memberHtml = GetMethodName(obj, member);
                return $"{memberHtml}{paramHtml}";
            }
        }
    }
}
