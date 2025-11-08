using FluentAssertions;
using NUnit.Framework;

namespace Markdown;

[TestFixture]
class TokenParser_Tests
{
    private void ParseText(string actualInput, Token[] expectedTokens)
    {
        var parser = new TokenParser();
        var actualTokens = parser.ParseTokens(actualInput);
        var act = () => Md.GenerateHtml(actualInput, actualTokens);
        var actualHtml = Md.GenerateHtml(actualInput, actualTokens);
        var expectedHtml = Md.GenerateHtml(actualInput, expectedTokens);
        
        act.Should().NotThrow();
        expectedHtml.Should().Be(actualHtml);
        actualTokens.Should().BeEquivalentTo(expectedTokens);
    }
    
    #region basic tests        
    public static IEnumerable<TestCaseData> ParseSimpleText_Source()
    {
        yield return new TestCaseData(
            "__main title__\n__some bold text__",
            new []
            {
                new Token(Marks.Bold, "main title", 0, 14),
                new Token(Marks.Bold, "some bold text", 15, 33)
            }).SetName("Bold text");
        yield return new TestCaseData(
            "_main title_\n_some italic text_",
            new []
            {
                new Token(Marks.Italic, "main title", 0, 12),
                new Token(Marks.Italic, "some italic text", 13, 31)
            }).SetName("Italic text");
        yield return new TestCaseData(
            "# main title\n# some header text",
            new []
            {
                new Token(Marks.Header, "main title", 0, 12),
                new Token(Marks.Header, "some header text", 13, 31)
            }).SetName("Headers text");
        yield return new TestCaseData(
            "- __bold__\n- _italic_\n- # header",
            new []
            {
                new Token(Marks.List, "<strong>bold</strong>", 0, 10),
                new Token(Marks.Bold, "bold", 2, 10),
                new Token(Marks.List, "<em>italic</em>", 11, 21),
                new Token(Marks.Italic, "italic", 13, 21),
                new Token(Marks.List, "<h1>header</h1>", 22, 32)
            }
        ).SetName("List with tags inside");
        yield return new TestCaseData(
            "# - 123\n",
            new []
            {
                new Token(Marks.Header, "<li>123</li>", 0, 8),
            }
        ).SetName("List inside header");
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
                new Token(Marks.Header, "<strong>main title</strong>", 0, 16),
                new Token(Marks.Bold, "main title", 2, 16),
                new Token(Marks.Bold, "some bold text", 17, 35)
            }).SetName("Inside header");
        yield return new TestCaseData(
            "# __main title__\n# __some _bold_ text__",
            new []
            {
                new Token(Marks.Header, "<strong>main title</strong>", 0, 16),
                new Token(Marks.Bold, "main title", 2, 16),
                new Token(Marks.Header, "<strong>some <em>bold</em> text</strong>", 17, 39),
                new Token(Marks.Bold, "some <em>bold</em> text", 19, 39),
                new Token(Marks.Italic, "bold", 26, 32)
            }).SetName("Italic inside Bold");
    }
        
    [Test, TestCaseSource(nameof(ParseNestingText_Source))]
    public void ParseText_OnNestingText(string actualInput, Token[] expectedTokens)
    {
        ParseText(actualInput, expectedTokens);
    }
    # endregion    
    
    public static IEnumerable<TestCaseData> ParseTextWithExceptions_Source()
    {
        yield return new TestCaseData(
            "внутри _одинарного __двойное__ не_ работает",
            new []
            {
                new Token(Marks.Italic, "одинарного __двойное__ не", 7, 34),
                new Token(Marks.Bold, "двойное", 19, 30)
            }).SetName("Bold inside Italic");
        yield return new TestCaseData(
            "c цифрами_12_3 не считаются выделением __даже1так__",
            new Token[] { }
        ).SetName("Numbers aren't tagged");
        yield return new TestCaseData(
            "эти__ подчерки__ не считаются и эти __подчерки __не считаются",
            new Token[] { }
        ).SetName("Spaces after/before bold mark");
        yield return new TestCaseData(
            "эти_ подчерки_ не считаются и эти _подчерки _не считаются",
            new Token[] { }
        ).SetName("Spaces after/before italic mark");
        yield return new TestCaseData(
            "выделение в ра_зных сл_овах н__е работ__ает",
            new Token[] { }
        ).SetName("Words splitted");
        yield return new TestCaseData(
            "__Непарные символы в рамках одного абзаца не считаются выделением",
            new Token[] { }
        ).SetName("Use only pairs of marks");
        yield return new TestCaseData(
            "пустая строка ____",
            new Token[] { }
        ).SetName("Empty text inside tags");
        yield return new TestCaseData(
            "__пересечения _двойных__ и __одинарных_ подчерков__",
            new Token[] { }
        ).SetName("Crossing marks");
        yield return new TestCaseData(
            "_пересечения __двойных_ и _одинарных__ подчерков_",
            new Token[] { }
        ).SetName("Crossing marks reversed");
        yield return new TestCaseData(
            @"экран\_ирование\_ и \\__двойное\\__ экрани\рование",
            new Token[]
            {
                new Token(Marks.Bold, @"двойное\\", 22, 35)
            }
        ).SetName("Shielding marks");
    }
        
    [Test, TestCaseSource(nameof(ParseTextWithExceptions_Source))]
    public void ParseText_WithExceptions(string actualInput, Token[] expectedTokens)
    {
        ParseText(actualInput, expectedTokens);
    }
}