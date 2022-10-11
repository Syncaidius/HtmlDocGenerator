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
        public override string GetTitle()
        {
            return "Methods";
        }

        protected override string GetMemberHtml(string ns, DocObject obj, MethodInfo member, bool isIndex)
        {
            if (member.IsSpecialName)
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

                        paramHtml += HtmlHelper.GetHtmlName(pi.ParameterType);
                    }
                }

                string memberHtml = base.GetMemberHtml(ns, obj, member, isIndex);
                return $"{memberHtml}({paramHtml})";
            }
        }
    }
}
