namespace HtmlDocGenerator;

public class DocTypeParameter : DocElement
{
    public DocTypeParameter(Type type) : base(type.Name, DocObjectType.TypeParameter)
    {
        ParameterType = type;
    }

    public Type ParameterType { get; }

    public override string Namespace => ParameterType.Namespace;
}
