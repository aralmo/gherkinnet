using System;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using GherkinNet.Language;
using System.IO;

namespace GherkinNet.Tests
{
    public class ValidationSpecification
    {
        [Fact(DisplayName = "feature and scenario sections should have a name")]
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
    }
}
