namespace Markdown;

public static class TagNames {
    public const string Strong = "strong";
    public const string Em = "em";
    public const string Header = "h1";
    public const string List = "li";
}

public static class TagFactory
{
    public static Tag Bold => new(TagNames.Strong);
    public static Tag Italic => new(TagNames.Em);
    public static Tag Header => new(TagNames.Header);
    public static Tag List => new(TagNames.List);

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
                throw new ArgumentException("Wrong mark!");
        }
    }
}