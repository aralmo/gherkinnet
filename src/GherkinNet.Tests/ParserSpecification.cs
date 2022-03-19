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

namespace GherkinNet.Tests
{
    public class ParserSpecification
    {
        [Fact(DisplayName = "Parse basic sections without content")]
        [Trait("language","parser")]
        public void given_some_sections_parser_should_return_dom()
        {
            string example
                = @"
                    feature:feature name
                    background: background name
                    scenario: scenario name
                   ";

            var dom = GherkinParser.Parse(new StringReader(example));
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
                = @"
                    feature:feature name
                        this is just text
                        more explanatory text                    
                        yet more comments                    
                   ";

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

            var dom = GherkinParser.Parse(new StringReader(example));
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

            var dom = GherkinParser.Parse(new StringReader(example));
            
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
            var dom = GherkinParser.Parse(new StringReader(example));
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
    }
}
