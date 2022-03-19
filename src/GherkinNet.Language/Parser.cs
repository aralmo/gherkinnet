using GherkinNet.Language.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GherkinNet.Language
{   
    public static class GherkinParser
    {
        const string SECTIONS_REGEX = @"(?:\s*)?(background:|scenario:|feature:)(.*)(\r|\n)?";
        const string NOUNS_REGEX = @"(?:\s*)?(given|when|then) (.*)(\r|\n)?";

        public static GherkinDOM Parse(TextReader reader)
            => new GherkinDOM()
            {
                Nodes = ParseLines(reader).ToArray()
            };

        static IEnumerable<Node> ParseLines(TextReader reader)
        {
            SectionNode section = null;
            string line = reader.ReadLine();
            int pos = 0;
            while (line != null)
            {
                //ignore empty and whitespace lines
                if (!string.IsNullOrWhiteSpace(line))
                {

                    if (tryParseSection(line, pos, out SectionNode sectnode))
                    {
                        section = sectnode;
                        yield return section;
                    }
                    else
                    {
                        bool isnoun = false;
                        foreach (var node in parseNoun(line, pos, section))
                        {
                            isnoun = true;
                            yield return node;
                        }

                        if (!isnoun)
                            yield return new TextNode()
                            {
                                Parent = section,
                                SourceIndex = pos,
                                SourceLength = line.Length,
                                Content = line
                            };
                    }
                    pos += line.Length;
                }
                line = reader.ReadLine();
            }

        }

        private static IEnumerable<Node> parseNoun(string line, int posOffset, SectionNode section)
        {
            var match = Regex.Match(line, NOUNS_REGEX);
            if (match.Success)
            {
                var noun = new NounNode()
                {
                    Parent = section,
                    SourceIndex = match.Groups[1].Index + posOffset,
                    SourceLength = match.Groups[1].Length,
                    Noun = (Nouns)Enum.Parse(typeof(Nouns),match.Groups[1].Value)
                };

                bool hasSentence = !string.IsNullOrWhiteSpace(match.Groups[2].Value);
                
                if (hasSentence)//parse and bind changes the noun, better to yield after
                    parseAndBindNounSentence(posOffset, noun, match);

                yield return noun;
                if (hasSentence)
                    yield return noun.Sentence;
            }
        }

        static void parseAndBindNounSentence(int posOffset, NounNode noun, Match match)
        {
            var sentence = new PendingSentence()
            {
                Parent = noun,
                SourceIndex = match.Groups[2].Index + posOffset,
                SourceLength = match.Groups[2].Length,
                Content = match.Groups[2].Value
            };
            noun.Sentence = sentence;
        }

        private static bool tryParseSection(string line, int posOffset, out SectionNode section)
        {
            var match = Regex.Match(line, SECTIONS_REGEX);
            if (match.Success)
            {
                section = new SectionNode()
                {
                    SourceIndex = match.Groups[1].Index + posOffset,
                    SourceLength = match.Groups[1].Length,
                    Type =(Sections) Enum.Parse(typeof(Sections), match.Groups[1].Value.Remove(match.Groups[1].Length - 1, 1)),//remove the : from the  feature type
                    Title = match.Groups[2].Value.Trim(),
                };

                return true;
            }
            else
            {
                section = null;
                return false;
            }
        }
    }
}
