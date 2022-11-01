namespace LunchBot;

public class IgnoreData
{
    public readonly IReadOnlyList<string> NamesToIgnore;

    public IgnoreData(IReadOnlyList<string> namesToIgnore)
    {
        NamesToIgnore = namesToIgnore;
    }
}