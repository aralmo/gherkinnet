using GherkinNet.Language.Binding;

namespace GherkinNet.Language.Nodes
{
    /// <summary>
    /// A sentence binded to an execution function
    /// </summary>
    public class BindedSentence : SentenceNode
    {
        public SentenceBinder Binder { get; set; }
    }
}
