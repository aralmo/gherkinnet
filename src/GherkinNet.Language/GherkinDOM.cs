using GherkinNet.Language.Binding;
using GherkinNet.Language.Nodes;
using GherkinNet.Language.Validation;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GherkinNet.Language
{
    public class GherkinDOM
    {
        public Node[] Nodes { get; private set; }
        public SentenceBinder[] SentenceBinders { get; private set; }

        internal GherkinDOM(Node[] nodes, SentenceBinder[] binders)
        {
            Nodes = nodes;
            SentenceBinders = binders;
        }

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

                    case NounNode noun:
                        foreach (var r in NodeRules.NounRules(noun))
                            yield return r;
                        break;

                    case PendingSentence sentence:
                        foreach (var r in NodeRules.PendingSentenceRules(sentence))
                            yield return r;
                        break;

                    case BindedSentence sentence:
                        foreach (var r in NodeRules.BindedSentenceRules(sentence))
                            yield return r;
                        break;
                }
            }
        }

    }
}
