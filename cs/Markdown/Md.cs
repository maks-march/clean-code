namespace Markdown
{
    public class Md
    {
        // Моя идея заключается в том, что найдя все действующие "inline elements"
        // сохранить позиции их содержимого в Token-ы чтобы при сборке html
        // поочередно вставлять в StringBuilder html-тэги из токенов и исходный текст.
        
        public static string Render(string input)
        {
            throw new Exception();
        }
        
        private static string GenerateHtml(string text, List<Token> tokens)
        {
            throw new Exception();            
        }
    }
}


