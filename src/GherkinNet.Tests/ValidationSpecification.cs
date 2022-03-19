using System;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using GherkinNet.Language;
using System.IO;
using GherkinNet.Language.Binding;
using GherkinNet.Language.Nodes;

namespace GherkinNet.Tests
{
    public class ValidationSpecification
    {
        [Fact(DisplayName = "feature and scenario sections should have a title")]
        [Trait("language", "validation")]
        public void given_feature_with_no_name_should_validate()
        {
            string example
                = @"
                    feature:
                        this is just text
                        more explanatory text                    
                        yet more comments                    
                    scenario: valid one
                    scenario:
                   ";

            var dom = GherkinParser.Parse(new StringReader(example));
            var errors = dom.Validate();
            errors.Should().HaveCount(2);
            
            errors.First().Node.Should().Be(dom.Nodes[0]);
            errors.First().Message.Should().Contain("should have a title");
            
            errors.Last().Node.Should().Be(dom.Nodes[5]);
            errors.Last().Message.Should().Contain("should have a title");
        }

        [Fact(DisplayName = "background section should not need a title")]
        [Trait("language", "validation")]
        public void background_should_not_require_title()
        {
            string example
                = @"
                    feature: f
                        this is just text
                        more explanatory text                    
                        yet more comments   
                    background: some title
                    scenario: valid one
                    background:
                   ";

            var dom = GherkinParser.Parse(new StringReader(example));
            var errors = dom.Validate();
            errors.Should().BeEmpty();
        }

        [Fact(DisplayName = "all nouns should be binded")]
        [Trait("language", "validation")]
        public void should_not_allow_pending_sentences()
        {
            string example
                = @"
                    scenario: some scenario
                        when I do something
                        then should get not binded error
                   ";

            var dom = GherkinParser.Parse(new StringReader(example));
            var errors = dom.Validate();
            errors.Should().HaveCount(2);
            errors.Should().AllSatisfy(e => e.Message.Contains("should be binded"));
        }

        [Fact(DisplayName = "all nouns should have body")]
        [Trait("language", "validation")]
        public void should_not_allow_empty_nouns()
        {
            string example
                = @"
                    scenario: some scenario
                        when 
                   ";

            var dom = GherkinParser.Parse(new StringReader(example));
            var errors = dom.Validate();
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Contain("should have a sentence");
        }


        [Fact(DisplayName = "Given an expression with wrong parameter count when validating show an error on the node using it")]
        [Trait("binding", "validation")]
        public static void binding_wrong_parameter_count_yields_error_on_validation()
        {
            string example = " given should be something";

            var shouldbe_method = typeof(BinderHelperSpecification)
                .GetMethod(nameof(ShouldBe), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            //the expression lacks one parameter
            var binding = BindingHelper.FromMethod(Nouns.given, "should be something", shouldbe_method);

            var dom = GherkinParser.Parse(example, new[] { binding! });
            var errors = dom.Validate();

            errors.Should().HaveCount(1);
            errors.First().Message.Should().Contain("should have the same parameter count");
        }
        static void ShouldBe(string something) { }

    }
}
