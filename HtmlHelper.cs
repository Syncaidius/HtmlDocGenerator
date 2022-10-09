using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public static class HtmlHelper
    {
        public static string GetHtmlName(string name)
        {
            return name.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static string GetHtmlName(Type t)
        {
            string[] gParts = t.Name.Split('`');
            string name = gParts[0];
            Type[] gTypes = t.GetGenericArguments();

            if (gTypes.Length > 0)
            {
                name += '<';
                for (int i = 0; i < gTypes.Length; i++)
                {
                    Type gType = gTypes[i];
                    name += $"{(i == 0 ? "" : ", ")}{GetHtmlName(gType)}";
                }
                name += '>';
            }

            return name;
        }
    }
}
