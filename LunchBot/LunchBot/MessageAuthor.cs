using System.Text;
using Microsoft.Extensions.Configuration;

namespace LunchBot;

public class MessageAuthor
{
    private readonly string _initialMessageFormat;
    private readonly string _reminderMessageFormat;

    public MessageAuthor(IConfigurationRoot configuration)
    {
        _initialMessageFormat = configuration.GetValue<string>("MessageFormat");
        _reminderMessageFormat = configuration.GetValue<string>("ReminderFormat");
    }

    public string CreateInitialChatMessage(Group group, string venue)
    {
        return CreateInitialChatMessage(group.Select(u => u.Name), venue);
    }
    
    public string CreateInitialChatMessage(IEnumerable<string> names, string venue)
    {
        string formattedNames = _initialMessageFormat.Contains("{0}") ? GetFormattedNames(names) : "";
        venue = _initialMessageFormat.Contains("{1}") ? venue : "";
        
        return string.Format(_initialMessageFormat, formattedNames, venue);
    }

    public string CreateTestInitialChatMessage()
    {
        return CreateInitialChatMessage(new[] { "Charlie", "Mac", "Dennis", "Dee", "Frank", "Cricket" }, "Dummy Venue");
    }

    public string CreateReminderMessage()
    {
        return string.Format(_reminderMessageFormat);
    }

    private static string GetFormattedNames(IEnumerable<string> names)
    {
        string[] orderedNames = names.OrderBy(n => n).ToArray();

        StringBuilder namesBuilder = new();

        for (int i = 0; i < orderedNames.Length; i++)
        {
            string name = orderedNames[i];
            namesBuilder.Append(name);

            if (i == orderedNames.Length - 2)
            {
                namesBuilder.Append(orderedNames.Length == 2 ? " and " : ", and ");
            }
            else if (i < orderedNames.Length - 2)
            {
                namesBuilder.Append(", ");
            }
        }

        return namesBuilder.ToString();
    }
}