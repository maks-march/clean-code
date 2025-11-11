using System.Text;

namespace Markdown;

public class Md
{
    // Моя идея заключается в том, что найдя все действующие "inline elements"
    // сохранить позиции их содержимого в Token-ы чтобы при сборке html
    // поочередно вставлять в StringBuilder html-тэги из токенов и исходный текст.

    // В итоге получился довольно большой парсер, но идея работает
    public static string Render(string input)
    {
        var parser = new TokenParser();
        var tokens = parser.ParseTokens(input);

        return GenerateHtml(input, tokens);
    }

    public static string GenerateHtml(string text, IEnumerable<Token> tokens)
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


