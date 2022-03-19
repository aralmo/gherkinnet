using System;
using System.Collections.Generic;
using System.Text;

namespace GherkinNet.Language.Nodes
{
    public abstract class SentenceNode:Node
    {
        public string Content;
    }

    /// <summary>
    /// A noun sentence that doesn't have a defined step
    /// </summary>
    public class PendingSentence : SentenceNode
    {
    }

    public class BindedSentence : SentenceNode
    {
        public SentenceBinder Binder { get; set; }
    }
}
