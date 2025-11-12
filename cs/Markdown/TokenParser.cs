using System.Collections;

namespace Markdown;

public class TokenParser
{
    private List<Token> _tokens;
    private HashSet<Token> _allFindedTokens;
    private string _text;
    private ParserValidator _validator;
    private Stack<PositionedTag> _tagStack;

    public TokenParser(string text = "")
    {
        _text = text;
        _validator = new (text);
        _tokens = new ();
        _allFindedTokens = new ();
        _tagStack = new ();
    }
    
    public IEnumerable<Token> ParseTokens(string input, string outerTokenMark = "")
    {
        _text = input;
        _validator = new ParserValidator(input);
        _tokens = new List<Token>();
        _allFindedTokens = new HashSet<Token>();
        _tagStack = new Stack<PositionedTag>();

        _tokens.AddRange(FindAllTokens());
        
        _tokens.Sort((x, y) => x.StartPosition.CompareTo(y.StartPosition));
        return _tokens;
    }

    private IEnumerable<Token> FindAllTokens()
    {
        for (int i = 0; i < _text.Length; i++)
        {
            if (_text[i] == '\n')
            {
                foreach (var token in BuildTokensFromStack(i))
                {
                    yield return token;
                }
                continue;
            }
            
            var findedTag = FindTag(i);
            if (findedTag == null)
                continue;
            
            // что-то уже лежит
            if (_tagStack.Count > 0)
            {
                var lastTag = _tagStack.Pop();
                if (lastTag.Name == findedTag.Name)
                {
                    // types equal check if open && close
                    if (lastTag.IsOpening && (!findedTag.IsOpening || findedTag.IsOpenClose))
                    {
                        var token = BuildTokenOrNull(lastTag, findedTag);
                        if (token is not null)
                            yield return token;
                    }
                    else
                        // two open or close in a row -> use second opener and first closer
                        if (lastTag.Name == TagNames.Header || lastTag.Name == TagNames.List)
                            _tagStack.Push(lastTag);
                        PushIfOpened(findedTag);
                }
                else
                {
                    if (lastTag.Mark != Marks.Italic)
                    {
                        _tagStack.Push(lastTag);
                        _tagStack.Push(findedTag);
                    }
                }
            }
            else
                PushIfOpened(findedTag);
        }
        foreach (var token in BuildTokensFromStack(_text.Length))
        {
            yield return token;
        }
    }

    private IEnumerable<Token> BuildTokensFromStack(int lineEnd)
    {
        while (_tagStack.Count > 0)
        {
            var current = _tagStack.Pop();
            if (current.Mark == Marks.Header || current.Mark == Marks.List)
            {
                var token = BuildTokenOrNull(current, new PositionedTag(lineEnd, current.Mark, false));
                if (token is not null)
                    yield return token;
            }
            
        }
    }

    private void PushIfOpened(PositionedTag findedTag)
    {
        if (findedTag.IsOpening)
            _tagStack.Push(findedTag);
    }

    private PositionedTag? FindTag(int index)
    {
        foreach (var mark in Marks.AllMarks)
        {
            var tag = GetPositionedTagOrNull(index, mark);
            if (tag is not null) 
                return tag;
        }

        return null;
    }
    
    #region GetPositionedTag
    
    private PositionedTag? GetPositionedTagOrNull(int index, string mark)
    {
        var isOpening = CheckByMark(index, mark, true);
        var isClosing = CheckByMark(index, mark, false);
        if (isOpening && isClosing)
        {
            return new PositionedTag(index, mark, isOpenClose:true);
        }
        if (isOpening)
            return new PositionedTag(index, mark, isOpening:isOpening);
        if (isClosing)
            return new PositionedTag(index, mark, isOpening:isOpening);
        
        return null;
    }

    private bool CheckByMark(int index, string mark, bool isOpening)
    {
        switch (mark)
        {
            case Marks.Bold:
                return CheckMarkForBold(index, isOpening);
            case Marks.Italic:
                return CheckMarkForItalic(index, isOpening);
            case Marks.Header:
                return CheckMarkForHeader(index, isOpening);
            case Marks.List:
                return CheckMarkForList(index, isOpening);
            default:
                return false;
        }
    }

    private bool CheckMarkForBold(int index, bool isOpening)
    {
        return index + 1 < _text.Length
               && _text[index] == '_'
               && _text[index + 1] == '_'
               && _validator.IsMarkCorrect(index, isOpening, Marks.Bold.Length);
    }
    
    private bool CheckMarkForItalic(int index, bool isOpening)
    {
        return index < _text.Length
               && _text[index] == '_'
               && !_validator.IsDoubleUnderscore(index)
               && _validator.IsMarkCorrect(index, isOpening, Marks.Italic.Length);
    }
    
    private bool CheckMarkForHeader(int index, bool isOpening)
    {
        return isOpening
               && index + 1 < _text.Length
               && _text[index].ToString() == Marks.Header
               && char.IsWhiteSpace(_text[index + 1]);
    }
    
    private bool CheckMarkForList(int index, bool isOpening)
    {
        return isOpening 
               && index + 1 < _text.Length
               && _text[index].ToString() == Marks.List
               && char.IsWhiteSpace(_text[index+1]);
    }
    
    #endregion

    private Token? BuildTokenOrNull(PositionedTag startTag, PositionedTag endTag)
    {
        var mark = startTag.Mark;
        var start = startTag.Position;
        var end = endTag.Position;
                    
        var content = _text.Substring(start + mark.Length + Marks.AfterMarkSpace(mark), endTag.Position - start - mark.Length - Marks.AfterMarkSpace(mark));
        
        if (!_validator.IsContentAcceptable(content, mark) || _validator.IsSplittingWords(start, end + mark.Length))
        {
            return null;
        }
        
        var token = new Token(
            TagFactory.BuildTag(mark),
            content,
            start,
            end
        );
        
        // возможно рудимент
        _allFindedTokens.Add(token);
        if (!SolveOverllaping(start, end))
            return token;
        return null;
    }
    
    private bool SolveOverllaping(int start, int end)
    {
        var isOverlapping = false;
        foreach (var token in _allFindedTokens)
        {
            if (
                end > token.EndPosition
                && token.StartPosition < start && start < token.EndPosition
                || token.StartPosition < end && end < token.EndPosition
                                             && start < token.StartPosition
            )
            {
                _tokens.Remove(token);
                isOverlapping = true;
            }
        }
        return isOverlapping;
    }
    
    # region commented
    /*
    private void AddHeaderTokens(string outerTokenMark = "")
    {
        _tokens.AddRange(FindParagraphTokens(Marks.Header));
    }
    
    private void AddListTokens(string outerTokenMark = "")
    {
        _tokens.AddRange(FindParagraphTokens(Marks.List));
    }
    
    private void AddBoldTokens(string outerTokenMark)
    {
        if (outerTokenMark != Marks.Italic && outerTokenMark != Marks.Bold)
        {
            _tokens.AddRange(FindTokens(
                (i, isOpeningTag) => _text[i] == '_' && _text[i+1] == '_' && _validator.IsMarkCorrect(i, isOpeningTag, Marks.Bold.Length),
                Marks.Bold
            ));
        }
    }
    private void AddItalicTokens(string outerTokenMark)
    {
        if (outerTokenMark != Marks.Italic)
        {
            _tokens.AddRange(FindTokens(
                (i, isOpeningTag) => _text[i] == '_' && !_validator.IsDoubleUnderscore(i) && _validator.IsMarkCorrect(i, isOpeningTag, Marks.Italic.Length),
                Marks.Italic
            ));
        }
    }
    

    private IEnumerable<Token> FindTokens(Func<int, bool, bool> checkMark, string mark)
    {
        var stack = new Stack<int>();

        for (int i = 0; i < _text.Length+1 - mark.Length; i++)
        {
            var isOpeningTag = stack.Count == 0;

            if (checkMark(i, isOpeningTag))
            {
                if (stack.Count > 0)
                {
                    var start = stack.Pop();
                    var end = i + mark.Length;
                    
                    var content = _text.Substring(start + mark.Length, i - start - mark.Length);
                    
                    if (!_validator.IsContentAcceptable(content) || _validator.IsSplittingWords(start, end))
                    {
                        continue;
                    }
                    var htmlContent = Md.GenerateHtml(content, new TokenParser().ParseTokens(content, mark));
                    var token = new Token(
                        TagFactory.BuildTag(mark),
                        htmlContent,
                        start,
                        end
                    );
                    
                    _allFindedTokens.Add(token);
                    if (SolveOverllaping(start, end))
                        yield return token;
                }
                else
                {
                    stack.Push(i);
                }
                i++;
            }
        }
    }
    
    private IEnumerable<Token> FindParagraphTokens(string mark)
    {
        // Обрабатываем текст построчно для заголовков
        int lineStart = 0;
    
        for (int i = 0; i < _text.Length; i++)
        {
            if (_text[i] == '\n' || i == _text.Length - 1)
            {
                // Определяем конец строки
                int lineEnd = (i == _text.Length - 1) ? i + 1 : i;
                foreach (var token in ParseLineToTokens(lineStart, lineEnd, mark))
                    yield return token;
            
                lineStart = i + 1;
            }
        }
    
        // Обрабатываем последнюю строку, если текст не заканчивается \n
        foreach (var token in ParseLineToTokens(lineStart, _text.Length, mark))
            yield return token;
    }

    private IEnumerable<Token> ParseLineToTokens(int lineStart, int lineEnd, string mark)
    {
        int lineLength = lineEnd - lineStart;
            
        if (lineLength > 0)
        {
            foreach (var token in FindTokensInLine(lineStart, lineEnd, mark))
            {
                _allFindedTokens.Add(token);
                if (SolveOverllaping(token.StartPosition, token.EndPosition))
                    yield return token;
            }
        }
    }
    
    private IEnumerable<Token> FindTokensInLine(int lineStart, int lineEnd, string mark)
    {
        // Если есть # и пробел
        int pos = lineStart;
        while (pos < lineEnd - 1 && _text[pos].ToString() != mark && char.IsWhiteSpace(_text[pos+1]))
        {
            pos++;
        }
        
        if (pos < lineEnd && _text[pos].ToString() == mark)
        {
            // Пропускаем пробел после #
            pos++;
        
            string content = _text.Substring(pos, lineEnd - pos).Trim();
            var htmlContent = Md.GenerateHtml(content, new TokenParser().ParseTokens(content, mark));

            yield return new Token(
                TagFactory.BuildTag(mark),
                htmlContent,
                pos - 1,
                lineEnd
                );
        }
    }
    */
    #endregion
}