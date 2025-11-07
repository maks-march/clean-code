using FluentAssertions;
using NUnit.Framework;

namespace Markdown;

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