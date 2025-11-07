namespace Markdown
{

    public class Tag
    {
        private string name;
        public string Name { get { return name; } }
        public string OpenTag => $"<{Name}>";
        public string CloseTag => $"</{Name}>";

        public Tag(string name)
        {
            this.name = name;
        }
    }
}