using Newtonsoft.Json;
using System.Reflection;

namespace HtmlDocGenerator;

[JsonObject(MemberSerialization.OptIn)]
public class DocPropertyMember : DocMember
{
    public DocPropertyMember(DocObject parent, PropertyInfo info) : base(parent, info)
    {
        Info = info;
        TypeName = $"{Namespace}.{HtmlHelper.GetHtmlName(Info.PropertyType)}";

        if (Info.GetMethod != null)
            Getter = new DocMethodMember(parent, Info.GetMethod);

        if (Info.SetMethod != null)
            Setter = new DocMethodMember(parent, Info.SetMethod);

        ParameterInfo[] indexParameters = info.GetIndexParameters();
        IsIndexer = indexParameters.Length > 0;

        for (int i = 0; i < indexParameters.Length; i++)
        {
            DocParameter p = new DocParameter(indexParameters[i]);
            Parameters.Add(p);
            ParametersByName.Add(p.Name, p);
        }
    }

    public PropertyInfo Info { get; }

    public string TypeName { get; }

    [JsonProperty]
    public DocMethodMember Getter { get; }

    [JsonProperty]
    public DocMethodMember Setter { get; }

    [JsonProperty]
    public bool IsIndexer { get; }

    public List<DocParameter> Parameters { get; } = new List<DocParameter>();

    public Dictionary<string, DocParameter> ParametersByName { get; } = new Dictionary<string, DocParameter>();

    [JsonProperty("Params")]
    public List<DocParameter> SerializedParameters => Parameters.Count > 0 ? Parameters : null;

    public override string Namespace => Info.PropertyType.Namespace;
}
