using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace GherkinNet.Tests
{
    public class TextParserPOC
    {
        [Fact]
        public void TextParserPOC1()
        {
            string example = "this is a text\nand another line\r\nand yet another line";

        }

        readonly struct Token
        {
        }
        enum TokenType
        {
            Word,
            Number,
            Keyword,
            Space,
            LineBreak
        }
    }
}
