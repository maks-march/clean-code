using System.Collections;

namespace Markdown
{
    public class TokenParser
    {
        private List<Token> _tokens;
        private HashSet<Token> _allFindedTokens;
        private string _text;
        private ParserValidator _validator;

        public TokenParser(string text = "")
        {
            this._text = text;
            _validator = new ParserValidator(text);
            _tokens = new List<Token>();
            _allFindedTokens = new HashSet<Token>();
        }
        
        public IEnumerable<Token> ParseTokens(string input, string outerTokenMark = "")
        {
            this._text = input;
            _validator = new ParserValidator(input);
            _tokens = new List<Token>();
            _allFindedTokens = new HashSet<Token>();
            
            AddHeaderTokens();
            AddListTokens();
            AddBoldTokens(outerTokenMark);
            AddItalicTokens(outerTokenMark);
            
            _tokens.Sort((x, y) => x.StartPosition.CompareTo(y.StartPosition));
            return _tokens;
        }
        
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
                            HtmlTagFactory.BuildTag(mark),
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
                    HtmlTagFactory.BuildTag(mark),
                    htmlContent,
                    pos - 1,
                    lineEnd
                    );
            }
        }
        
        private bool SolveOverllaping(int start, int end)
        {
            var overlapping = GetOverlappingTokens(start, end);
            if (!overlapping.Any())
            {
                return true;
            }
            else
            {
                ClearOverlappingTokens(overlapping);
                return false;
            }
        }
        
        private IEnumerable<Token> GetOverlappingTokens(int start, int end)
        {
            var overlappingTokens = new List<Token>();
            foreach (var token in _allFindedTokens)
            {
                if (
                    end > token.EndPosition
                    && token.StartPosition < start && start < token.EndPosition
                    || token.StartPosition < end && end < token.EndPosition
                    && start < token.StartPosition
                )
                {
                    overlappingTokens.Add(token);
                }
            }
            return overlappingTokens;
        }

        private void ClearOverlappingTokens(IEnumerable<Token> toDelete)
        {
            foreach (var tokenToRemove in toDelete)
            {
                _tokens.Remove(tokenToRemove);
            }
        }
    }
}