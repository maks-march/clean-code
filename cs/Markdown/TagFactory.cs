namespace Markdown
{
    public static class TagFactory
    {
        private static Tag Bold => new("strong");
        private static Tag Italic => new("em");
        private static Tag Title => new("h1");

        public static Tag BuildTag(string mark)
        {
            throw new Exception();
        }
    }
}