using Newtonsoft.Json;

namespace HtmlDocGenerator;

[JsonObject(MemberSerialization.OptIn)]
public class DocNamespace : DocElement
{
    public override string Namespace => Name;

    public DocNamespace(string name) : base(name, DocObjectType.Namespace)
    {
        Name = name;
    }
}
