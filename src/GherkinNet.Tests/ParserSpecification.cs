using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using GherkinNet.Language;
using System.IO;
using FluentAssertions;
using GherkinNet.Language.Nodes;
using System.Threading;
using GherkinNet.Language.Binding;
using System.Text.RegularExpressions;

namespace GherkinNet.Tests
{
    public class ParserSpecification
    {
        [Fact(DisplayName = "Parse basic sections without content")]
        [Trait("language", "parser")]
        public void given_some_sections_parser_should_return_dom()
        {
            string example
                = @"feature:feature name
                    background: background name
                    scenario: scenario name";

            var dom = GherkinParser.Parse(example);
            dom.Nodes.Count().Should().Be(3);
            dom.Nodes.Should().AllBeOfType<SectionNode>();


            dom.Nodes.Should<Node>().AllSatisfy(node =>
            {
                node.Should().BeOfType<SectionNode>();
                //title should be 'section name'
                (node as SectionNode)!.Title.Should().Be($"{(node as SectionNode)!.Type} name");
            });
        }

        [Fact(DisplayName = "Parse assign children description text to section")]
        [Trait("language", "parser")]
        public void given_feature_section_and_sometext_we_get_correct_dom()
        {
            string example
                = @"feature:feature name
                        this is just text
                        more explanatory text                    
                        yet more comments";

            var dom = GherkinParser.Parse(new StringReader(example));
            dom.Nodes.Count().Should().Be(4);
            dom.Nodes[0].Should().BeOfType<SectionNode>();
            dom.Nodes.Skip(1).Should().AllBeOfType<TextNode>();
            dom.Nodes.Skip(1).Should().AllSatisfy(node => node.Parent.Should().Be(dom.Nodes[0]));
        }

        [Fact(DisplayName = "Parsing an incomplete noun returns a noun without sentence")]
        [Trait("language", "parser")]
        public void should_get_a_noun_withouth_statement()
        {
            string example
                = @"
                    scenario: some
                        when 
                   ";

            var dom = GherkinParser.Parse(example);
            dom.Nodes[1].Should().BeOfType<NounNode>();
            (dom.Nodes[1] as NounNode)!.Sentence.Should().BeNull();
        }

        [Fact(DisplayName = "Parsing a noun statement inside a feature should return textnode")]
        [Trait("language", "parser")]
        public void parsing_inside_feature_should_return_textnode()
        {
            string example
                = @"
                    feature: some feature
                        when i write this
                        then should get textnodes instead
                   ";

            var dom = GherkinParser.Parse(new StringReader(example));
            dom.Nodes.Skip(1).Should().AllBeOfType<TextNode>();
        }

        [Fact(DisplayName = "Parsing one line noun yields correct")]
        [Trait("language", "parser")]
        public void parsing_only_noun_yields_correct()
        {
            string example
               = @"when we parse this";

            var dom = GherkinParser.Parse(example);
            dom.Nodes.Should().HaveCount(2);
            dom.Nodes.First().Should().BeOfType<NounNode>();
            dom.Nodes.Last().Should().BeOfType<PendingSentence>();
        }

        [Fact(DisplayName = "Parse parents multiple features with children")]
        [Trait("language", "parser")]
        public void given_two_sections_parenting_is_correct()
        {
            string example
                = @"
                    background:feature name
                        given something
                        given something else
                    
                    scenario: some scenario
                        when I do something
                        then something else should happen                        
                   ";

            var dom = GherkinParser.Parse(example);

            //we don't care of the nouns decomposed nodes for this test
            var nodes = dom.Nodes.Where(x => x is SectionNode || x is NounNode).ToArray();

            nodes.Count().Should().Be(6);//this noun should have 2 each expressions

            nodes[0].Should().BeOfType<SectionNode>();
            nodes[1].Should().BeOfType<NounNode>();
            nodes[1].Parent.Should().Be(nodes[0]);
            nodes[2].Should().BeOfType<NounNode>();
            nodes[2].Parent.Should().Be(nodes[0]);

            nodes[3].Should().BeOfType<SectionNode>();
            nodes[4].Should().BeOfType<NounNode>();
            nodes[4].Parent.Should().Be(nodes[3]);
            nodes[5].Should().BeOfType<NounNode>();
            nodes[5].Parent.Should().Be(nodes[3]);
        }

        [Fact(DisplayName = "given no context, nouns sentences decompose into pending to implement nodes")]
        [Trait("language", "parser")]
        public void when_not_giving_context_nouns_decompose_to_pending()
        {
            string example
                = @"
                    background:feature name
                        given something
                        given something else        
                   ";
            var dom = GherkinParser.Parse(example);
            var nodes = dom.Nodes;

            nodes[0].Should().BeOfType<SectionNode>();
            nodes[1].Should().BeOfType<NounNode>();
            nodes[1].Parent.Should().Be(nodes[0]);
            nodes[2].Should().BeOfType<PendingSentence>();
            nodes[2].Parent.Should().Be(nodes[1]);
            nodes[3].Should().BeOfType<NounNode>();
            nodes[3].Parent.Should().Be(nodes[0]);
            nodes[4].Should().BeOfType<PendingSentence>();
            nodes[4].Parent.Should().Be(nodes[3]);
        }

        [Fact(DisplayName = "Async parser stops parsing if cancellation was requested")]
        [Trait("language", "parser")]
        public void given_10_lines_parser_stops_on_cancellation_request()
        {
            var sb = new StringBuilder();
            for (int n = 0; n < 10000; n++)
                sb.AppendLine("a line \n");
            string example = sb.ToString();

            var source = new CancellationTokenSource();
            var r = GherkinParser.ParseAsync(new StringReader(example), null, source.Token);
            var awaiter = r.GetAwaiter();
            source.Cancel();

            var result = awaiter.GetResult();
            result.Nodes.Should().HaveCountLessThan(10000);
        }

        [Fact(DisplayName = "Parser parses a correctly binded sentence and arguments when given")]
        [Trait("language", "parser")]
        public void given_valid_match_parsed_binded_sentence()
        {
            string example
            = @"
                    scenario:some scenarioname
                        given something we have
                        when we do something
                        then we have a result
                   ";
            var methodinfo = typeof(ParserSpecification).GetMethod(nameof(testMethod), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var dom = GherkinParser.Parse(example, new SentenceBinder[]
            {
                BindingHelper.FromMethod(Nouns.given,"(.*) we have",methodinfo),
                BindingHelper.FromMethod(Nouns.when,"we do (.*)",methodinfo),
                BindingHelper.FromMethod(Nouns.then,"we have (.*)",methodinfo)
            });

            var nodes = dom.Nodes;
            nodes.Should().AllSatisfy(node => node.Should().NotBeOfType<PendingSentence>());
            nodes.Where(n => n is BindedSentence).Should().HaveCount(3);
            nodes.Where(n => n is BindedSentence).Should().AllSatisfy(node =>
            {
                Assert.IsType<BindedSentence>(node);
                var binded = node as BindedSentence;
                if (binded! != null)
                {
                    binded.Binder.ParameterTypes.Should().HaveCount(1);
                    binded.Binder.ParameterNames.Should().HaveCount(1);
                    binded.Binder.ParameterTypes[0].Should().Be(typeof(string));
                }
            });

        }
        static void testMethod(string parameter) { }

        [Fact(DisplayName = "Parser selects the binded sentence according to the noun")]
        [Trait("language", "parser")]
        public void given_multiple_options_selects_bind_for_noun()
        {
            string example
            = @"
                    scenario:some scenarioname
                        given something
                        when something
                        then something
                   ";

            var binders = new SentenceBinder[]
            {
                new SentenceBinder()
                {
                    Noun = Nouns.given,
                    RegularExpression = "something"
                },
                new SentenceBinder()
                {
                    Noun = Nouns.when,
                    RegularExpression = "something"
                },
                new SentenceBinder()
                {
                    Noun = Nouns.then,
                    RegularExpression = "something"
                }
            };
            var dom = GherkinParser.Parse(example, binders);

            var nodes = dom.Nodes.Where(n => n is SentenceNode).ToArray();
            nodes.Should().AllBeOfType<BindedSentence>();
            nodes.Should().HaveCount(3);

            nodes.Select<Node, BindedSentence>(n => (n as BindedSentence)!).Should()
                .AllSatisfy(node => node.Binder.Noun.Should().Be((node.Parent as NounNode)!.Noun));

        }

        [Fact(DisplayName = "When sending a change to a DOM object the parser correctly applies changes to the nodes.")]
        [Trait("language", "parser")]
        public void given_a_change_the_dom_updates_correctly()
        {
            string example
            = @"
                    scenario:some scenarioname
                        given something
                        when something
                        then something
                   ";

            var binders = new SentenceBinder[]
            {
                new SentenceBinder()
                {
                    Noun = Nouns.given,
                    RegularExpression = "something"
                },
                new SentenceBinder()
                {
                    Noun = Nouns.when,
                    RegularExpression = "something"
                },
                new SentenceBinder()
                {
                    Noun = Nouns.then,
                    RegularExpression = "something"
                }
            };
            var dom = GherkinParser.Parse(example, binders);

            //assert the  first state is correct
            var nodes = dom.Nodes.Where(n => n is SentenceNode).ToArray();
            nodes.Should().AllBeOfType<BindedSentence>();
            nodes.Should().HaveCount(3);
            nodes.Select<Node, BindedSentence>(n => (n as BindedSentence)!).Should()
                .AllSatisfy(node => node.Binder.Noun.Should().Be((node.Parent as NounNode)!.Noun));

            dom.Apply("given crap", example.IndexOf("given"), "given something".Length);

            (dom.Nodes[2] as NounNode).Sentence.Should().BeOfType<PendingSentence>();
        }

        [Fact(DisplayName = "Given a script the sourceindex on the parsed nodes should be correct")]
        [Trait("language", "parser")]
        public void given_a_script_nodes_parse_correct_source_index()
        {
            string example = "\r\ntext\ntext2\r\ntext3";

            var dom = GherkinParser.Parse(example);

            dom.Nodes[0].SourceIndex.Should().Be(2);
            dom.Nodes[1].SourceIndex.Should().Be(7);
            dom.Nodes[2].SourceIndex.Should().Be(14);
        }

        [Fact]
        public void line_parser_poc()
        {
            var example = "this is a line\nandanother\r\nathirdone";
            var result = ParseTextLines(new StringReader(example)).ToArray();
            result[0].line.Should().Be("this is a line");
            result[0].position.Should().Be(0);
            result[1].line.Should().Be("andanother");
            result[1].position.Should().Be(example.IndexOf("and"));
            result[2].line.Should().Be("athirdone");
            result[2].position.Should().Be(example.IndexOf("athirdone"));
            
            result.Should().HaveCount(3);
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
    }
}
