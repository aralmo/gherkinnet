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
                switch (node)
                {
                    case SectionNode section:
                        foreach (var r in NodeRules.SectionRules(section))
                            yield return r;
                    break;
                }
            }
        }

    }
}
