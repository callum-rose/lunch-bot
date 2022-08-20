using LunchBot;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Graph;
using Serilog;

namespace LunchBotCLI;

[Command(Name = "remind", Description = "Send a reminder to groups that haven't said anything yet",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
internal class RemindGroupsCommand : CommandBase
{
    [Option(ShortName = "l", Description = "The absolute path of the lunch data file")]
    private string LunchDataPath { get; set; }

    [Option(CommandOptionType.NoValue, ShortName = "d", Description = "Is this not a test")]
    private bool IsNotDryRun { get; set; } = false;

    private bool IsDryRun => !IsNotDryRun;

    private readonly GraphServiceClient _graphServiceClient;
    private readonly LunchDataFiler _lunchDataFiler;
    private readonly LunchDataHelper _lunchDataHelper;
    private readonly ILogger _logger;
    private readonly MessageAuthor _messageAuthor;
    private readonly ChatHandler _chatHandler;

    public RemindGroupsCommand(GraphServiceClient graphServiceClient, LunchDataFiler lunchDataFiler,
        LunchDataHelper lunchDataHelper, ILogger logger, MessageAuthor messageAuthor, ChatHandler chatHandler)
    {
        _graphServiceClient = graphServiceClient;
        _lunchDataFiler = lunchDataFiler;
        _lunchDataHelper = lunchDataHelper;
        _logger = logger;
        _messageAuthor = messageAuthor;
        _chatHandler = chatHandler;
    }

    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        if (string.IsNullOrWhiteSpace(LunchDataPath))
        {
            if (!_lunchDataHelper.TryPromptForLunchData(out string lunchDataPath))
            {
                _logger.Information("Exiting");
                return await CommandHelper.ExecuteRootCommand(app);
            }

            LunchDataPath = lunchDataPath;
        }

        if (IsDryRun)
        {
            Console.WriteLine("Doing dry run");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("NOT A TEST, THIS IS THE REAL SHIT!!");
            Console.ResetColor();
        }

        LunchData lunchData = await _lunchDataFiler.Load(LunchDataPath);

        if (lunchData.WasDryRun)
        {
            Console.WriteLine("FYI this lunch was a dry run");
        }
        
        if (!lunchData.Successful)
        {
            Console.WriteLine("FYI this lunch was not successful");
        }
        
        if (!Blocker.RequestUserCodeVerification("Enter code to send reminders"))
        {
            _logger.Warning("Incorrect code entered. Cancelling reminder");
            return await CommandHelper.ExecuteRootCommand(app);
        }

        await SendReminders(lunchData);

        _logger.Information("Finished sending reminder messages");

        return await CommandHelper.ExecuteRootCommand(app);
    }

    private async Task SendReminders(LunchData lunchData)
    {
        User conductor = await _graphServiceClient.Me.Request().GetAsync();

        int failedCount = 0;

        foreach (string chatId in lunchData!.Chats.Select(c => c.ChatId))
        {
            bool chatUsersHaveMessaged = await ChatUsersHaveMessaged(chatId, conductor);

            if (chatUsersHaveMessaged)
            {
                continue;
            }

            bool success = await SendReminder(chatId);

            if (!success)
            {
                failedCount++;
            }
        }

        if (failedCount > 0)
        {
            _logger.Error($"Failed to send reminder to {failedCount} chats");
        }
    }

    private async Task<bool> SendReminder(string chatId)
    {
        string message = _messageAuthor.CreateReminderMessage();

        bool success = await _chatHandler.TrySendMessage(IsDryRun, chatId, message);
        return success;
    }

    private async Task<bool> ChatUsersHaveMessaged(string chatId, User conductor)
    {
        IChatMessagesCollectionPage messages;

        try
        {
            messages = await _graphServiceClient.Chats[chatId].Messages.Request().GetAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Could not find chat: {chatId}");
            return false;
        }

        bool IsNotEvent(ChatMessage m) => m.EventDetail is null;
        bool IsFromUser(ChatMessage m) => m.From.User.Id != conductor.Id;

        bool hasSpoken = messages.Where(IsNotEvent).Any(IsFromUser);
        return hasSpoken;
    }
}