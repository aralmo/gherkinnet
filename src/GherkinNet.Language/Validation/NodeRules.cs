using GherkinNet.Language.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GherkinNet.Language.Validation
{
    internal static class NodeRules
    {
        internal static IEnumerable<ValidationResult> SectionRules(SectionNode section)
        {
            //any section other than background should have a title
            if (section.Type != Sections.background && string.IsNullOrEmpty(section.Title))
                yield return new ValidationResult(section, $"{section.Type} should have a title");
        }
    }
}
