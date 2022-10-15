using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocMethodMember : DocMember
    {
        public override void Set(DocObject obj, string name, Type[] parameters = null, Type[] genericParameters = null)
        {
            BindingFlags bFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            Info = obj.UnderlyingType.GetMethod(name, genericParameters.Length, bFlags, null, parameters, null);
            if(Info != null)
            {
                Parameters = parameters ?? new Type[0];
                GenericParameters = genericParameters ?? new Type[0];
            }
        }

        public override bool IsMatch(DocObject obj, string name, Type[] parameters = null, Type[] genericParameters = null)
        {
            bool parametersMatch = TypeArrayMatch(Parameters, parameters);
            bool genericsMatch = TypeArrayMatch(GenericParameters, genericParameters);

            return parametersMatch && genericsMatch && name == Info.Name;
        }

        private bool TypeArrayMatch(Type[] a, Type[] b)
        {
            bool match = a == b;
            if (a != null)
            {
                match = match && a.Length == Parameters.Length;

                if (match)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        if (a[i].Name != b[i].Name)
                            return false;
                    }
                }
            }

            return match;
        }

        public Type[] Parameters { get; protected set; }

        public Type[] GenericParameters { get; protected set; }
    }
}
