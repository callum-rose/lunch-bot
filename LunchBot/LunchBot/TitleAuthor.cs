using Microsoft.Extensions.Configuration;

namespace LunchBot;

public class TitleAuthor
{
    private readonly string _titleFormat;

    public TitleAuthor(IConfigurationRoot configuration)
    {
        _titleFormat = configuration.GetValue<string>("ChatTitleFormat");
    }

    public string GetTitle(string partyName)
    {
        return string.Format(_titleFormat, partyName);
    }
}