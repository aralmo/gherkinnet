using GherkinNet.Language.Nodes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GherkinNet.Language.Validation
{
    internal static class NodeRules
    {
        internal static IEnumerable<ValidationResult> SectionRules(SectionNode section)
        {
            //any section other than background should have a title
            if (section.Type != Sections.background && string.IsNullOrEmpty(section.Title))
                yield return new ValidationResult(section, $"'{section.Type}' should have a title");
        }

        internal static IEnumerable<ValidationResult> NounRules(NounNode node)
        {
            //all nouns should have a sentence
            if (node.Sentence == null)
                yield return new ValidationResult(node, $"'{node.Noun}' should have a sentence");

        }

        internal static IEnumerable<ValidationResult> BindedSentenceRules(BindedSentence sentence)
        {
            string error = null;
            Regex regex = null;
            try
            {
                regex = new Regex(sentence.Binder.RegularExpression);

            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error != null)
                yield return new ValidationResult(sentence, $"Error validating the binder regular expression {error}");
            else
                if (regex.GetGroupNumbers().Length-1 != sentence.Binder.ParameterNames.Length)
                    yield return new ValidationResult(sentence, $"binded expression should have the same parameter count");
        }

        internal static IEnumerable<ValidationResult> PendingSentenceRules(PendingSentence node)
        {
            //Pending sentence rules should always be binded, this node type is not allowed
            yield return new ValidationResult(node, $"'{(node.Parent as NounNode).Noun}' sentence '{node.Content}' should be binded");
        }
    }
}
