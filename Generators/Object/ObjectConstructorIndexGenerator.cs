using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class ObjectConstructorIndexGenerator : ObjectMemberSectionGenerator<ConstructorInfo>
    {
        public override string GetTitle()
        {
            return "Constructors";
        }

        protected override string GetMemberHtml(string ns, DocObject obj, ConstructorInfo member, bool isIndex)
        {
            string paramHtml = "";
            string memberHtml = HtmlHelper.GetHtmlName(obj.Name);

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

                return $"{memberHtml}({paramHtml})";
            }

            return memberHtml;
        }
    }
}
