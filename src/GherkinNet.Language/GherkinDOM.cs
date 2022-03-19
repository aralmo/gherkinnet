using GherkinNet.Language.Nodes;
using GherkinNet.Language.Validation;
using System;
using System.Collections.Generic;

namespace GherkinNet.Language
{
    public class GherkinDOM
    {
        public Node[] Nodes;

        public IEnumerable<ValidationResult> Validate()
        {
            if (Nodes == null)
                yield break;

            foreach (var node in Nodes)
            {
                if (node is SectionNode section && string.IsNullOrEmpty(section.Title))
                    yield return new ValidationResult(node, $"{section.Type} should have a title");
            }
        }

    }
}
