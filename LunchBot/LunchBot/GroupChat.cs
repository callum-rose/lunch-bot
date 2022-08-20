using Newtonsoft.Json;

namespace LunchBot;

[JsonObject]
public class GroupChat : Group
{
    public string ChatId { get; }
    public string Venue { get; }

    public GroupChat(IReadOnlyList<MyUser> users, int number, string chatId, string venue) : base(users, number)
    {
        ChatId = chatId;
        Venue = venue;
    }
}