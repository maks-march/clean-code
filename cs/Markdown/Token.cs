using System.Text;
using NUnit.Framework;

namespace Markdown;

public class Token
{
    private Tag tag;
    public string Head => tag.OpenTag;
    public string Tail => tag.CloseTag;
    public int StartPosition { get; }

    public int EndPosition { get; }
    public int SpaceAfterTag => Marks.AfterMarkSpace(Marks.GetMarkByTagName(tag.Name));
    public int MarkLength => Marks.GetMarkByTagName(tag.Name).Length;

    public int PushForward => 2;
    public string Content { get; }

    public Token(Tag tag, string content, int start, int end)
    {
        this.tag = tag;
        Content = content;
        StartPosition = start;
        EndPosition = end;
    }

    public Token(string mark, string content, int start, int end) : this(TagFactory.BuildTag(mark), content, start, end)
    {
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Head);
        builder.Append(Content);
        builder.Append(Tail);
        return builder.ToString();
    }
}