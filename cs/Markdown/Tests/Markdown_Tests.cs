using FluentAssertions;
using NUnit.Framework;

namespace Markdown;

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