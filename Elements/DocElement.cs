using Newtonsoft.Json;

namespace HtmlDocGenerator;

[JsonObject(MemberSerialization.OptIn)]
public abstract class DocElement
{
    public class NameComparer : IComparer<DocElement>
    {
        public int Compare(DocElement x, DocElement y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }

    public DocElement(string name, DocObjectType initialType)
    {
        Name = name;
        DocType = initialType;
    }

    [JsonProperty]
    public string Summary { get; set; }

    [JsonProperty]
    public string Remark { get; set; }

    public abstract string Namespace { get; }

    [JsonProperty]
    public DocObjectType DocType { get; set; }

    public void AddMember(DocElement element)
    {
        if (!Members.TryGetValue(element.Name, out List<DocElement> memList))
        {
            memList = new List<DocElement>();
            Members.Add(element.Name, memList);
        }

        memList.Add(element);
    }

    [JsonProperty]
    public bool? IsStatic { get; init; }

    [JsonProperty]
    public bool? IsVirtual { get; init; }

    [JsonProperty]
    public bool? IsAbstract { get; init; }

    [JsonProperty]
    public bool? IsProtected { get; init; }


    public Dictionary<string, List<DocElement>> Members { get; set; } = new Dictionary<string, List<DocElement>>();

    /// <summary>
    /// A null-if-zero-count version of <see cref="Members"/>. This is used for simplifying serialization.
    /// </summary>
    [JsonProperty("Members")]
    public object Elements => Members.Count > 0 ? Members : null;

    public virtual string Name { get; set; }
}
