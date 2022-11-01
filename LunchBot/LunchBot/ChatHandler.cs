using Microsoft.Graph;
using Serilog;

namespace LunchBot;

public class ChatHandler
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ILogger _logger;

    public ChatHandler(GraphServiceClient graphServiceClient, ILogger logger)
    {
        _graphServiceClient = graphServiceClient;
        _logger = logger;
    }

    public async Task<(bool success, string chatId)> TryCreateChat(bool dryRun, User conductor, string title,
        Group group)
    {
        Chat request = CreateChatData(conductor, title, group);

        if (dryRun)
        {
            _logger.Information($"Dry run create chat for group {group.Number}");
            return (true, "test");
        }

        Chat response;

        try
        {
            response = await _graphServiceClient.Chats.Request().AddAsync(request);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Error creating chat for group {group.Number}");
            return (false, "error");
        }

        _logger.Information($"Created chat for group {group.Number} with id {response.Id}");
        return (true, response.Id);
    }

    public async Task<bool> TrySendMessage(bool dryRun, string chatId, string messageText)
    {
        ChatMessage message = CreateMessageData(messageText);

        if (dryRun)
        {
            _logger.Information("Dry run send message");
            return true;
        }

        try
        {
            await _graphServiceClient.Chats[chatId].Messages.Request().AddAsync(message);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Error sending message \"{messageText}\" to chat {chatId}");
            return false;
        }

        _logger.Information($"Sent message \"{messageText}\" to chat {chatId}");
        return true;
    }

    private Chat CreateChatData(User conductor, string title, Group group)
    {
        ChatMembersCollectionPage chatMembers = new();

        foreach (MyUser user in group)
        {
            AadUserConversationMember chatMember = CreateConversationMember(user.Id);
            chatMembers.Add(chatMember);
        }

        bool groupContainsConductor = group.Any(u => u.Id == conductor.Id);

        if (!groupContainsConductor)
        {
            AadUserConversationMember chatMember = CreateConversationMember(conductor.Id);
            chatMembers.Add(chatMember);
        }

        return new Chat
        {
            ChatType = ChatType.Group,
            Topic = title,
            Members = chatMembers
        };
    }

    private AadUserConversationMember CreateConversationMember(string userId)
    {
        return new AadUserConversationMember
        {
            Roles = new List<string> { "owner" },
            AdditionalData = new Dictionary<string, object>()
            {
                { "user@odata.bind", $"https://graph.microsoft.com/v1.0/users('{userId}')" }
            }
        };
    }

    private ChatMessage CreateMessageData(string text)
    {
        return new ChatMessage
        {
            Body = new ItemBody()
            {
                ContentType = BodyType.Html,
                Content = text
            }
        };
    }
}