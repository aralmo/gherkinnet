using GherkinNet.Language.Binding;
using GherkinNet.Language.Nodes;
using GherkinNet.Language.Validation;
using System;
using System.Linq;
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

        //ToDo:would like this to be returning a new dom instead but parent references prevent it
        public void Apply(string code, int sourceIndex, int sourceLength)
        {

        //ToDo: needs refactor enumerable etc.
            var newParsed = GherkinParser.Parse(code,SentenceBinders);
            foreach (var n in newParsed.Nodes)
                n.SourceIndex += sourceIndex;

            int endIndex = sourceIndex + sourceLength;
            int sizeDelta = code.Length - sourceLength;
            bool afterReplacing = false;
            List<Node> result = new List<Node>();
            bool replacing = false;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i].SourceIndex >= sourceIndex && Nodes[i].SourceIndex < endIndex)
                {
                    //node needs to change and if first inject the compiled code
                    replacing = true;
                    //do not add this node
                    continue;
                }
                else
                {
                    if (replacing)
                    {
                        //add the new nodes here
                        result.AddRange(newParsed.Nodes);
                        replacing = false;
                        afterReplacing = true;
                    }
                }

                if (afterReplacing)
                    Nodes[i].SourceIndex += sizeDelta;

                result.Add(Nodes[i]);
            }

            Nodes = result.ToArray();
        }
    }
}
