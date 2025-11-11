using System.Text;
using NUnit.Framework;

namespace Markdown;

public class Token
{
    private string value;
    private Tag tag;
    public string Head => tag.OpenTag;
    public string Tail => tag.CloseTag;
    public int Length => value.Length;
    
    public int StartPosition { get; }

    public int EndPosition { get; }

    public int OriginalLength => EndPosition - StartPosition;

    public Token(Tag tag, string value, int start, int end)
    {
        this.tag = tag;
        this.value = value;
        this.StartPosition = start;
        this.EndPosition = end;
    }

    public Token(string mark, string value, int start, int end) : this(TagFactory.BuildTag(mark), value, start, end)
    {
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Head);
        builder.Append(value);
        builder.Append(Tail);
        return builder.ToString();
    }
}