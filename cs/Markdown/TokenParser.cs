namespace Markdown
{
    public class TokenParser
    {
        private List<Token> tokens = new();
        
        public IEnumerable<Token> ParseTokens(string input)
        {
            tokens = new List<Token>();
            tokens.AddRange(FindBoldTokens(input));
            tokens.AddRange(FindItalicTokens(input));
            tokens.AddRange(FindHeaderTokens(input));
            
            tokens.Sort((x, y) => x.StartPosition.CompareTo(y.StartPosition));
            return tokens;
        }

        private IEnumerable<Token> FindBoldTokens(string input)
        {
            var stack = new Stack<int>(); // Храним позиции открывающих тегов
        
            for (var i = 0; i < input.Length - 1; i++)
            {
                // Проверяем два символа подряд
                if (input[i] == '_' && input[i + 1] == '_')
                {
                    if (stack.Count > 0)
                    {
                        // Нашли закрывающий тег
                        var start = stack.Pop();
                        var end = i + 2; // +2 потому что два символа
                    
                        // Извлекаем содержимое (без __ и __)
                        var content = input.Substring(start + 2, i - start - 2);
                        var htmlContent = Md.GenerateHtml(content, new TokenParser().FindItalicTokens(content));
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
        
        private IEnumerable<Token> FindItalicTokens(string input)
        {
            var stack = new Stack<int>(); // Храним позиции открывающих тегов
        
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '_')
                {
                    // Проверяем, что это не часть двойного подчеркивания
                    bool isDoubleUnderscore = i < input.Length - 1 && input[i + 1] == '_';
                
                    if (!isDoubleUnderscore)
                    {
                        if (stack.Count > 0)
                        {
                            // Нашли закрывающий тег
                            int start = stack.Pop();
                            int end = i + 1;
                        
                            // Извлекаем содержимое (без _ и _)
                            string content = input.Substring(start + 1, i - start - 1);
                        
                            // Проверяем, что этот тег не пересекается с уже найденными
                            if (!IsOverlappingWithExisting(start, end))
                            {
                                yield return new Token(
                                    HtmlTagFactory.Italic,
                                    content,
                                    start,
                                    end
                                );
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
        
        private bool IsOverlappingWithExisting(int start, int end)
        {
            foreach (var token in tokens)
            {
                if (start < token.EndPosition && end > token.StartPosition)
                {
                    return true;
                }
            }
            return false;
        }
        
        private IEnumerable<Token> FindHeaderTokens(string input)
        {
            // Обрабатываем текст построчно для заголовков
            int lineStart = 0;
        
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\n' || i == input.Length - 1)
                {
                    // Определяем конец строки
                    int lineEnd = (i == input.Length - 1) ? i + 1 : i;
                    int lineLength = lineEnd - lineStart;
                
                    if (lineLength > 0)
                    {
                        foreach (var token in ProcessHeaderLine(input, lineStart, lineEnd))
                        {
                            yield return token;
                        }
                    }
                
                    lineStart = i + 1; // Начало следующей строки
                }
            }
        
            // Обрабатываем последнюю строку, если текст не заканчивается \n
            if (lineStart < input.Length)
            {
                foreach (var token in ProcessHeaderLine(input, lineStart, input.Length))
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
                var htmlContent = Md.GenerateHtml(content, new TokenParser().ParseTokens(content));

                yield return new Token(
                    HtmlTagFactory.Title,
                    htmlContent,
                    pos - 1,
                    lineEnd
                    );
            }
        }
    }
}

