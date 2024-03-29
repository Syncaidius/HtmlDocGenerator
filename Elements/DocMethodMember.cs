﻿using Newtonsoft.Json;
using System.Reflection;

namespace HtmlDocGenerator;

[JsonObject(MemberSerialization.OptIn)]
public class DocMethodMember : DocMember
{
    public DocMethodMember(DocObject parent, MethodBase info) : base(parent, info)
    {
        IsStatic = info.IsStatic ? true : null;
        IsAbstract = info.IsAbstract ? true : null;
        IsVirtual = info.IsVirtual && !info.IsAbstract ? true : null;
        IsProtected = info.IsFamily && !info.IsPrivate ? true : null;

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

            if (info is MethodInfo methodInfo)
            {
                if(methodInfo.ReturnType != typeof(void))
                    ReturnTypeName = $"{methodInfo.ReturnType.Namespace}.{HtmlHelper.GetHtmlName(methodInfo.ReturnType)}";
            }
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

    [JsonProperty("Return")]
    public string ReturnTypeName { get; }

    public List<DocParameter> Parameters { get; } = new List<DocParameter>();

    [JsonProperty("Params")]
    public List<DocParameter> SerializedParameters => Parameters.Count > 0 ? Parameters : null;

    public Dictionary<string, DocParameter> ParametersByName { get; } = new Dictionary<string, DocParameter>();

    public List<DocTypeParameter> GenericParameters { get; } = new List<DocTypeParameter>();
}
