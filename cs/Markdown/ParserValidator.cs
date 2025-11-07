namespace Markdown;

public class ParserValidator
{
    private string text;

    public ParserValidator(string input)
    {
        text = input;
    }
    
    public bool IsMarkCorrect(int startIndex, bool isOpening, int markLength = 1)
    {
        var isScreened = (startIndex > 0 && text[startIndex - 1] == '\\') 
                         && (startIndex > 1 && text[startIndex - 2] != '\\' || startIndex == 1);
        if (isOpening)
        {
            return !isScreened 
                   && startIndex + markLength < text.Length 
                   && text[startIndex + markLength] != ' ';
        }
        else
        {
            return !isScreened 
                   && startIndex > 0
                   && text[startIndex - 1] != ' ';
        }
    }

    public bool IsDoubleUnderscore(int index)
    {
        return index < text.Length - 1 && text[index + 1] == '_'
               || index > 0 && text[index - 1] == '_';
    }

    public bool IsContentAcceptable(string content)
    {
        return !string.IsNullOrEmpty(content) && HasNoDigits(content);
    }

    public bool IsSplittingWords(int start, int end)
    {
        return start > 0 && text[start - 1] != ' '
                && end < text.Length - 1 &&  text[end + 1] != ' '
                && text.Substring(start, end - start).Contains(' ');
    }
    
    public bool HasNoDigits(string content)
    {
        foreach (char c in content)
        {
            if (char.IsDigit(c))
                return false;
        }
        return true;
    }
    
    
}