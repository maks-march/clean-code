namespace Markdown
{
    public static class HtmlTagFactory
    {
        public static Tag Bold => new("strong");
        public static Tag Italic => new("em");
        public static Tag Title => new("h1");
        public static Tag MarkedList => new("li");


        public static Tag BuildTag(string mark)
        {
            switch (mark)
            {
                case "#":
                    return Title;
                case "__":
                    return Bold;
                case "_":
                    return Italic;
                case "*":
                    return MarkedList;
                default:
                    throw new Exception("Wrong mark!");
            }
        }
    }
}