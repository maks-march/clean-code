using System.Text;

namespace Markdown;

public class Md
{
    public static string Render(string input)
    {
        var parser = new TokenParser();
        var tokens = parser.ParseTokens(input);

        return GenerateHtml(input, tokens);
    }

    public static string GenerateHtml(string text, IEnumerable<Token> tokens)
    {
        var mergedText = new StringBuilder();

        var tokenStack = new Stack<Token>();
        var prevPosition = 0;
        var outerEnd = text.Length + 3;
        foreach (var token in tokens)
        {
            if (token.EndPosition > outerEnd)
            {
                mergedText.Append(text.Substring(prevPosition,token.EndPosition - prevPosition - 2));
                mergedText.Append(token.Tail);
                prevPosition = token.EndPosition;
            }
            else
            {
                mergedText.Append(text.Substring(prevPosition,token.StartPosition-prevPosition));
                mergedText.Append(token.Head);
                prevPosition = token.StartPosition + 2;
                outerEnd = token.EndPosition;
                tokenStack.Push(token);
            }
        }

        while (tokenStack.Count > 0)
        {
            var token = tokenStack.Pop();
            mergedText.Append(text.Substring(prevPosition - token.MarkLength,token.EndPosition - prevPosition));
            mergedText.Append(token.Tail);
            prevPosition = token.EndPosition + 2;
        }
        prevPosition -= 3;
        mergedText.Append(text.Substring(prevPosition, text.Length - prevPosition));
        return mergedText.ToString();
    }

    public static string GenerateHtmlOld(string text, IEnumerable<Token> tokens)
    {
        var mergedText = new StringBuilder();
        var prevPosition = 0;
        var nextPositionMin = -1;
        foreach (var token in tokens)
        {
            var currentPosition = token.StartPosition;
            if (currentPosition < nextPositionMin)
            {
                continue;
            }
            mergedText.Append(text.Substring(prevPosition,currentPosition-prevPosition));
            mergedText.Append(token.ToString());
            prevPosition = token.EndPosition;
            nextPositionMin = token.EndPosition;
        }
        mergedText.Append(text.Substring(prevPosition));
        return mergedText.ToString();        
    }
}


