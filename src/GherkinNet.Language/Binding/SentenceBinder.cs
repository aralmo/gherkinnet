using GherkinNet.Language.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GherkinNet.Language.Binding
{
    /// <summary>
    /// Represents a regular expression and other data required to bind a formal language sentence to a function
    /// </summary>
    public class SentenceBinder
    {
        public Nouns Noun;
        public string RegularExpression;
        public string[] ParameterNames;
        public Type[] ParameterTypes;
    }
}
