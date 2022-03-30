using GherkinNet.Language.Binding;
using GherkinNet.Language.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GherkinNet.Language
{
    public static class GherkinParser
    {
        const string SECTIONS_REGEX = @"(?:\s*)?(background:|scenario:|feature:)(.*)(\r|\n)?";
        const string NOUNS_REGEX = @"(?:\s*)?(given|when|then) (.*)(\r|\n)?";

        public static GherkinDOM Parse(string content, SentenceBinder[] binders = null)
            => Parse(new StringReader(content), binders);

        public static GherkinDOM Parse(TextReader reader, SentenceBinder[] binders = null)
            => new GherkinDOM(ParseLines(reader, binders).ToArray(), binders);

        public static async Task<GherkinDOM> ParseAsync(TextReader reader, SentenceBinder[] binders = null, CancellationToken? cancellationToken = null)
        {
            var nodes = await Task.Run(() =>
                ParseLines(reader, binders, cancellationToken).ToArray(), cancellationToken ?? default(CancellationToken));

            return new GherkinDOM(nodes, binders);
        }

        static IEnumerable<Node> ParseLines(TextReader reader, SentenceBinder[] binders = null, CancellationToken? cancellationToken = null)
        {

            //ToDo: needs refactor

            SectionNode section = null;

            foreach (var parsedLine in ParseTextLines(reader))
            {
                //ignore empty lines
                if (string.IsNullOrEmpty(parsedLine.line))
                    continue;

                if (cancellationToken?.IsCancellationRequested ?? false)
                    break;

                if (tryParseSection(parsedLine.line, parsedLine.position, out SectionNode sectnode))
                {
                    section = sectnode;
                    yield return section;
                }
                else
                {
                    bool isnoun = false;
                    //features only accept text
                    if (section?.Type != Sections.feature)
                        foreach (var node in parseNoun(parsedLine.line, parsedLine.position, section, binders))
                        {
                            isnoun = true;
                            yield return node;
                        }

                    if (!isnoun)
                        yield return new TextNode()
                        {
                            Parent = section,
                            SourceIndex = parsedLine.position,
                            SourceLength = parsedLine.line.Length,
                            Content = parsedLine.line
                        };
                }                
            }

        }
        const int MAX_LINE_LENGTH = 2048;
        private static IEnumerable<(string line, int position)> ParseTextLines(TextReader reader)
        {
            var c = reader.Read();
            char[] line_buffer = new char[MAX_LINE_LENGTH];
            int pos = 0;
            int line_start = 0;
            int length = 0;
            Func<(string, int)> currentLine = () => (new string(line_buffer, 0, length).TrimEnd('\r'), line_start);

            while (c > -1)
            {
                if (c == '\n')
                {
                    yield return currentLine();
                    line_start = pos + 1;
                    length = 0;
                }
                else
                {
                    length++;
                    line_buffer[pos - line_start] = (char)c;
                }

                pos++;
                c = reader.Read();
            }

            if (length > 0)
                yield return currentLine();
        }

        private static IEnumerable<Node> parseNoun(string line, int posOffset, SectionNode section, SentenceBinder[] binders)
        {
            var match = Regex.Match(line, NOUNS_REGEX);
            if (match.Success)
            {
                var noun = new NounNode()
                {
                    Parent = section,
                    SourceIndex = match.Groups[1].Index + posOffset,
                    SourceLength = match.Groups[1].Length,
                    Noun = (Nouns)Enum.Parse(typeof(Nouns), match.Groups[1].Value)
                };

                bool hasSentence = !string.IsNullOrWhiteSpace(match.Groups[2].Value);

                if (hasSentence)//parse and bind changes the noun, better to yield after
                    parseAndBindNounSentence(posOffset, noun, match, binders);

                yield return noun;
                if (hasSentence)
                    yield return noun.Sentence;
            }
        }
        static void parseAndBindNounSentence(int posOffset, NounNode noun, Match match, SentenceBinder[] binders)
        {
            SentenceNode sentence = null;
            string content = match.Groups[2].Value;

            var matchingBinder = binders?
                .Where(binder => binder.Noun == noun.Noun)
                .FirstOrDefault(binder => Regex.IsMatch(content, binder.RegularExpression));

            if (matchingBinder != null)
            {
                sentence = new BindedSentence()
                {
                    Parent = noun,
                    SourceIndex = match.Groups[2].Index + posOffset,
                    SourceLength = match.Groups[2].Length,
                    Content = content,
                    Binder = matchingBinder
                };
            }
            else
            {
                sentence = new PendingSentence()
                {
                    Parent = noun,
                    SourceIndex = match.Groups[2].Index + posOffset,
                    SourceLength = match.Groups[2].Length,
                    Content = content
                };
            }

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
                    Type = (Sections)Enum.Parse(typeof(Sections), match.Groups[1].Value.Remove(match.Groups[1].Length - 1, 1)),//remove the : from the  feature type
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
