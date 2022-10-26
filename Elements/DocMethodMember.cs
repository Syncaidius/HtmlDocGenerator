using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DocMethodMember : DocMember
    {
        public DocMethodMember(DocObject parent, MethodBase info) : base(parent, info)
        {
            ParameterInfo[] parameters = info.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                DocParameter p = new DocParameter(parameters[i]);
                Parameters.Add(p);
                ParametersByName.Add(p.Name, p);
            }

            if (info.MemberType != MemberTypes.Constructor)
            {
                Type[] gParams = info.GetGenericArguments();
                for (int i = 0; i < gParams.Length; i++)
                    GenericParameters.Add(new DocTypeParameter(gParams[i]));
            }
        }

        public override bool IsMatch(DocObject obj, string name, Type[] parameters = null, Type[] genericParameters = null)
        {
            bool parametersMatch = TypeListMatch(Parameters, parameters, p => p.Info.ParameterType);
            bool genericsMatch = TypeListMatch(GenericParameters, genericParameters, g => g.ParameterType);

            return parametersMatch && genericsMatch && name == BaseInfo.Name;
        }

        private bool TypeListMatch<T>(List<T> a, Type[] b, Func<T, Type> callback)
            where T : DocElement
        {
            bool match = true;

            if (a != null && b != null)
            {
                match = match && a.Count == b.Length;

                if (match)
                {
                    for (int i = 0; i < a.Count; i++)
                    {
                        Type t = callback(a[i]);

                        if (t.Name != b[i].Name)
                            return false;
                    }
                }
            }

            return match;
        }

        [JsonProperty]
        public List<DocParameter> Parameters { get; } = new List<DocParameter>();

        public Dictionary<string, DocParameter> ParametersByName { get; } = new Dictionary<string, DocParameter>();

        public List<DocTypeParameter> GenericParameters { get; } = new List<DocTypeParameter>();
    }
}
