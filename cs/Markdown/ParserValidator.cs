namespace Markdown;

public class ParserValidator
{
    private string _text;

    public ParserValidator(string input)
    {
        _text = input;
    }
    
    public bool IsMarkCorrect(int startIndex, bool isOpening, int markLength = 1)
    {
        var isScreened = (startIndex > 0 && _text[startIndex - 1] == '\\') 
                         && (startIndex > 1 && _text[startIndex - 2] != '\\' || startIndex == 1);
        if (isOpening)
        {
            return !isScreened 
                   && startIndex + markLength < _text.Length 
                   && _text[startIndex + markLength] != ' ';
        }
        else
        {
            return !isScreened 
                   && startIndex > 0
                   && _text[startIndex - 1] != ' ';
        }
    }

    public bool IsDoubleUnderscore(int index)
    {
        return index < _text.Length - 1 && _text[index + 1] == '_'
               || index > 0 && _text[index - 1] == '_';
    }

    public bool IsContentAcceptable(string content)
    {
        return !string.IsNullOrEmpty(content) && HasNoDigits(content);
    }

    public bool IsSplittingWords(int start, int end)
    {
        return start > 0 && _text[start - 1] != ' '
                && end < _text.Length - 1 &&  _text[end + 1] != ' '
                && _text.Substring(start, end - start).Contains(' ');
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