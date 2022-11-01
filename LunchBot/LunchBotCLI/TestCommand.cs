using LunchBot;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Serilog;

namespace LunchBotCLI;

[Command(Name = "test",
    Description = "test",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
internal class TestCommand : CommandBase
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly UserFinder _userFinder;
    private readonly AppDataFiler _appDataFiler;
    private readonly IConfigurationRoot _configuration;

    public TestCommand(GraphServiceClient graphServiceClient, UserFinder userFinder, AppDataFiler appDataFiler, IConfigurationRoot configuration)
    {
        _graphServiceClient = graphServiceClient;
        _userFinder = userFinder;
        _appDataFiler = appDataFiler;
        _configuration = configuration;
    }

    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        AppData? appData = await _appDataFiler.Load();
        var conductor = await _graphServiceClient.Me.Request().GetAsync();
        (_, MyUser? callum) = await _userFinder.GetUser(new HrPerson("Callum", "Rose", "Creative"), appData);

        Chat request = CreateChatData(conductor, callum!.Id);
        Chat? response = await _graphServiceClient.Chats.Request().AddAsync(request);

        ChatMessage message = CreateMessageData(string.Format(_configuration.GetValue<string>("MessageFormat"), "Person", "Venue"));
        ChatMessage? messageResponse =
            await _graphServiceClient.Chats[response.Id].Messages.Request().AddAsync(message);

        return await CommandHelper.ExecuteRootCommand(app);
    }

    private Chat CreateChatData(User conductor, string callum)
    {
        ChatMembersCollectionPage chatMembers = new();

        AadUserConversationMember callumMember = CreateConversationMember(callum);
        chatMembers.Add(callumMember);

        AadUserConversationMember conductorMember = CreateConversationMember(conductor.Id);
        chatMembers.Add(conductorMember);

        return new Chat
        {
            ChatType = ChatType.OneOnOne,
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