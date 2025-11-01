namespace Markdown
{
    public class Token
    {
        private string value;
        private Tag tag;
        public string Head => tag.OpenTag;
        public string Tail => tag.CloseTag;
        public int position;
        public int Length { get { return value.Length; } }


        public Token(Tag tag, string value)
        {

        }

        public Token(string mark, string value)
        {

        }

        private Tag RecognizeMark(string mark)
        {
            throw new Exception();
        }
    }
}