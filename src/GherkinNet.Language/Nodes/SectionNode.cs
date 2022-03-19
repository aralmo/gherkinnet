namespace GherkinNet.Language.Nodes
{
    public class SectionNode : Node
    {
        //todo: change into an enum or both to specific type
        public Sections Type;
        public string Title;
    }
    public enum Sections
    {
        background,
        feature,
        scenario
    }
}
