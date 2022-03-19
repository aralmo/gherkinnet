using FluentAssertions;
using GherkinNet.Language.Binding;
using GherkinNet.Language.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GherkinNet.Tests
{
    public class BinderHelperSpecification
    {
        [Fact(DisplayName = "BindingHelper creates a simple binding from method")]
        [Trait("binding", "helper")]
        public static void binding_helper_returns_correct_binding_for_method()
        {
            var method = typeof(BinderHelperSpecification)
                .GetMethod(nameof(ShouldBe), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            
            var binding = BindingHelper.FromMethod(Nouns.when, "should be (.*)", method);

            binding.Should().NotBeNull();
            binding.ParameterNames.Should().HaveCount(1);
            binding.ParameterNames[0].Should().Be("something");

            binding.ParameterTypes.Should().HaveCount(1);
            binding.ParameterTypes[0].Should().Be(typeof(string));
        }
        static void ShouldBe(string something){}


    }
}
