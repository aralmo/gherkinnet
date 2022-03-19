using GherkinNet.Language.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GherkinNet.Language.Binding
{
    public static class BindingHelper
    {
        public static SentenceBinder FromMethod(Nouns noun, string expression, MethodInfo method)
        {
            var parameters = method.GetParameters();            
            var match = Regex.Match(expression,expression);
            return new SentenceBinder()
            {
                Noun = noun,
                ParameterNames = parameters.AsEnumerable().Select(p => p.Name).ToArray(),
                ParameterTypes = parameters.AsEnumerable().Select(p => p.ParameterType).ToArray(),
                RegularExpression = expression
            };
        }
    }
}
