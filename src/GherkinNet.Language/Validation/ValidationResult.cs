using GherkinNet.Language.Nodes;
using System;

namespace GherkinNet.Language.Validation
{
    public readonly struct ValidationResult
    {
        public readonly Node Node;
        public readonly string Message;
        public readonly ValidationResultType Type;

        public ValidationResult(Node node, string message, ValidationResultType type = ValidationResultType.BuildError) : this()
        {
            Node = node;
            Message = message;
            this.Type = type;
        }
    }

    [Flags]
    public enum ValidationResultType
    {
        BuildError
    }
}
