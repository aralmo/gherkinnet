using GherkinNet.Language.Nodes;

namespace GherkinNet.Language.Validation
{
    public readonly struct ValidationResult
    {
        public readonly Node Node;
        public readonly string Message;

        public ValidationResult(Node node, string message) : this()
        {
            Node = node;
            Message = message;
        }

    }
}
