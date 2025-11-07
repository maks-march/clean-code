namespace Markdown
{
    public class TokenParser
    {
        private List<Token> tokens;
        private List<Token> allFindedTokens;
        private string text;
        private ParserValidator validator;

        public TokenParser(string text = "")
        {
            this.text = text;
            validator = new ParserValidator(text);
            tokens = new List<Token>();
            allFindedTokens = new List<Token>();
        }
        
        public IEnumerable<Token> ParseTokens(string input)
        {
            this.text = input;
            validator = new ParserValidator(input);
            tokens = new List<Token>();
            allFindedTokens = new List<Token>();
            
            tokens.AddRange(FindBoldTokens());
            tokens.AddRange(FindItalicTokens());
            tokens.AddRange(FindHeaderTokens());
            
            tokens.Sort((x, y) => x.StartPosition.CompareTo(y.StartPosition));
            return tokens;
        }
        
        private IEnumerable<Token> FindBoldTokens()
        {
            var stack = new Stack<int>(); // Храним позиции открывающих тегов
        
            for (var i = 0; i < text.Length - 1; i++)
            {
                var isOpeningTag = stack.Count == 0;
                // Проверяем два символа подряд
                if (text[i] == '_' && validator.IsDoubleUnderscore(i) && validator.IsMarkCorrect(i, isOpeningTag, 2))
                {
                    if (stack.Count > 0)
                    {
                        // Нашли закрывающий тег
                        var start = stack.Pop();
                        var end = i + 2; // +2 потому что два символа
                    
                        // Извлекаем содержимое ввиде текста и сразу делаем из него html
                        var content = text.Substring(start + 2, i - start - 2);
                        // Проверка на необходимость выделения
                        if (!validator.IsContentAcceptable(content) || validator.IsSplittingWords(start, end))
                        {
                            continue;
                        }
                        
                        // Вписывание вложенных тэгов
                        var htmlContent = Md.GenerateHtml(content, new TokenParser(content).FindItalicTokens());
                        
                        yield return new Token(
                            HtmlTagFactory.Bold,
                            htmlContent,
                            start,
                            end
                            );
                        i++; // Пропускаем второй символ
                    }
                    else
                    {
                        // Нашли открывающий тег
                        stack.Push(i);
                        i++; // Пропускаем второй символ
                    }
                }
            }
        }
        
        private IEnumerable<Token> FindItalicTokens()
        {
            // Использовал тэг для возможных закрывающих марок
            var stack = new Stack<int>(); // Храним позиции открывающих тегов
        
            for (int i = 0; i < text.Length; i++)
            {
                var isOpeningTag = stack.Count == 0;
                
                if (text[i] == '_' && validator.IsMarkCorrect(i, isOpeningTag))
                {
                    // Проверяем, что это не часть двойного подчеркивания
                    if (!validator.IsDoubleUnderscore(i))
                    {
                        if (stack.Count > 0)
                        {
                            // Нашли закрывающий тег
                            int start = stack.Pop();
                            int end = i + 1;
                        
                            // Извлекаем содержимое (без "марок")
                            string content = text.Substring(start + 1, i - start - 1);
                            
                            // Проверка на необходимость выделения
                            if (!validator.IsContentAcceptable(content) || validator.IsSplittingWords(start, end))
                            {
                                continue;
                            }
                            
                            var token = new Token(
                                HtmlTagFactory.Italic,
                                content,
                                start,
                                end
                            );
                            // Проверяем, что этот тег не пересекается с уже найденными
                            if (!CheckOverlappingWithExistingTokens(start, end))
                            {
                                yield return token;
                            }
                            else
                            {
                                allFindedTokens.Add(token);
                            }
                        }
                        else
                        {
                            // Нашли открывающий тег
                            stack.Push(i);
                        }
                    }
                }
            }
        }
        
        private IEnumerable<Token> FindHeaderTokens()
        {
            // Обрабатываем текст построчно для заголовков
            int lineStart = 0;
        
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n' || i == text.Length - 1)
                {
                    // Определяем конец строки
                    int lineEnd = (i == text.Length - 1) ? i + 1 : i;
                    int lineLength = lineEnd - lineStart;
                
                    if (lineLength > 0)
                    {
                        foreach (var token in ProcessHeaderLine(text, lineStart, lineEnd))
                        {
                            yield return token;
                        }
                    }
                
                    lineStart = i + 1; // Начало следующей строки
                }
            }
        
            // Обрабатываем последнюю строку, если текст не заканчивается \n
            if (lineStart < text.Length)
            {
                foreach (var token in ProcessHeaderLine(text, lineStart, text.Length))
                {
                    yield return token;
                }
            }
        }
        private IEnumerable<Token> ProcessHeaderLine(string input, int lineStart, int lineEnd)
        {
            // Если есть # и пробел
            int pos = lineStart;
            while (pos < lineEnd && input[pos] != '#' && !char.IsWhiteSpace(input[pos+1]))
            {
                pos++;
            }
            
            if (pos < lineEnd && input[pos] == '#')
            {
                // Пропускаем пробел после #
                pos++;
            
                // Извлекаем содержимое заголовка
                string content = input.Substring(pos, lineEnd - pos).Trim();
                var htmlContent = Md.GenerateHtml(content, new TokenParser(text).ParseTokens(content));

                yield return new Token(
                    HtmlTagFactory.Title,
                    htmlContent,
                    pos - 1,
                    lineEnd
                    );
            }
        }
        
        private bool CheckOverlappingWithExistingTokens(int start, int end)
        {
            var isOverlapping = false;
            var toDelete = new List<Token>();
            allFindedTokens.AddRange(tokens);
            foreach (var token in allFindedTokens)
            {
                if (
                    end > token.EndPosition
                    && token.StartPosition < start && start < token.EndPosition
                    || token.StartPosition < end && end < token.EndPosition
                    && start < token.StartPosition
                )
                {
                    // Удаляем токены пересекаемые найденным
                    toDelete.Add(token);
                    isOverlapping = true;
                }
            }
            ClearOverlappingToken(toDelete);
            return isOverlapping;
        }

        private void ClearOverlappingToken(IEnumerable<Token> toDelete)
        {
            foreach (var tokenToRemove in toDelete)
            {
                tokens.Remove(tokenToRemove);
            }
        }
    }
}