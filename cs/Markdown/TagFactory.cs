namespace Markdown
{
    public static class HtmlTagFactory
    {
        public static Tag Bold => new("strong");
        public static Tag Italic => new("em");
        public static Tag Header => new("h1");
        public static Tag List => new("li");


        public static Tag BuildTag(string mark)
        {
            switch (mark)
            {
                case Marks.Header:
                    return Header;
                case Marks.Bold:
                    return Bold;
                case Marks.Italic:
                    return Italic;
                case Marks.List:
                    return List;
                default:
                    throw new Exception("Wrong mark!");
            }
        }
    }
}