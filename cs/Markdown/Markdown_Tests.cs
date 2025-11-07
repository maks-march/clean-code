using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    class Markdown_Tests
    {
        public static IEnumerable<TestCaseData> GenerateHtmlSource()
        {
            yield return new TestCaseData(
                "# main title\n__some bold text__",
                "<h1>main title</h1>\n<strong>some bold text</strong>",
                new []
                {
                    new Token("#", "main title", 0, 12),
                    new Token("__", "some bold text", 13, 31)
                }
                ).SetName("Simple text");
            yield return new TestCaseData(
                "# main title\n__some _bold_ text__",
                "<h1>main title</h1>\n<strong>some <em>bold</em> text</strong>",
                new []
                {
                    new Token("#", "main title", 0, 12),
                    new Token("__", "some <em>bold</em> text", 13, 33)
                }
            ).SetName("Token inside token");
        }
        
        [Test, TestCaseSource(nameof(GenerateHtmlSource))]
        public void GenerateHtml_DifferentText(string actual, string expected, Token[] tokens)
        {
            var t = new TokenParser().ParseTokens(actual);
            Md.GenerateHtml(actual, tokens).Should().Be(expected);
        }
    }

    [TestFixture]
    class TokenParser_Tests
    {
        private void ParseText(string actualInput, Token[] expectedTokens)
        {
            var parser = new TokenParser();
            var actualTokens = parser.ParseTokens(actualInput);
            var act = () => Md.GenerateHtml(actualInput, actualTokens);
            
            act.Should().NotThrow();
            var lookUp = Md.GenerateHtml(actualInput, actualTokens);
            var lookUp2 = Md.GenerateHtml(actualInput, expectedTokens);
            actualTokens.Should().BeEquivalentTo(expectedTokens);
        }
        
        public static IEnumerable<TestCaseData> ParseSimpleText_Source()
        {
            yield return new TestCaseData(
                "__main title__\n__some bold text__",
                new []
                {
                    new Token("__", "main title", 0, 14),
                    new Token("__", "some bold text", 15, 33)
                }).SetName("Bold text");
            yield return new TestCaseData(
                "_main title_\n_some italic text_",
                new []
                {
                    new Token("_", "main title", 0, 12),
                    new Token("_", "some italic text", 13, 31)
                }).SetName("Italic text");
            yield return new TestCaseData(
                "# main title\n# some header text",
                new []
                {
                    new Token("#", "main title", 0, 12),
                    new Token("#", "some header text", 13, 31)
                }).SetName("Headers text");
        }

        [Test, TestCaseSource(nameof(ParseSimpleText_Source))]
        public void ParseText_OnSimpleText(string actualInput, Token[] expectedTokens)
        {
            ParseText(actualInput, expectedTokens);
        }
        
        public static IEnumerable<TestCaseData> ParseNestingText_Source()
        {
            yield return new TestCaseData(
                "# __main title__\n__some bold text__",
                new []
                {
                    new Token("#", "<strong>main title</strong>", 0, 16),
                    new Token("__", "some bold text", 15, 33)
                }).SetName("Inside header");
        }
        
        [Test, TestCaseSource(nameof(ParseNestingText_Source))]
        public void ParseText_OnNestingText(string actualInput, Token[] expectedTokens)
        {
            ParseText(actualInput, expectedTokens);
        }
    }

    [TestFixture]
    class Tags_Tests
    {
        [Test]
        public void Build_Recognize_OnWrongMark()
        {
            var act = () => HtmlTagFactory.BuildTag("1");
            act.Should().Throw();
        }

        [Test]
        [
            TestCase("#", "h1"),
            TestCase("_", "em"),
            TestCase("__", "strong"),
            TestCase("*", "li")
        ]
        public void Build_CorrectTag_OnCorrectMark(string mark, string correctName)
        {
            var tag = HtmlTagFactory.BuildTag(mark);
            var correctTag = new Tag(correctName);

            tag.Should().BeEquivalentTo(correctTag);
        }

        [Test]
        public void OpenCloseOutput_IsCorrect()
        {
            var tag = HtmlTagFactory.BuildTag("#");
            var text = tag.OpenTag + "text" + tag.CloseTag;
            
            text.Should().Be("<h1>text</h1>");
        }
    }
}