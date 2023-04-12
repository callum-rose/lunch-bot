using System.Text;
using LunchBot;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Serilog;
using ShellProgressBar;

namespace LunchBotCLI;

[Command(Name = "remind",
	Description = "Send a reminder to groups that haven't said anything yet",
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
	private readonly AppDataFiler _appDataFiler;
	private readonly int _minMessageCount;

	public RemindGroupsCommand(GraphServiceClient graphServiceClient, LunchDataFiler lunchDataFiler,
		LunchDataHelper lunchDataHelper, ILogger logger, MessageAuthor messageAuthor, ChatHandler chatHandler,
		AppDataFiler appDataFiler, IConfigurationRoot configurationRoot)
	{
		_graphServiceClient = graphServiceClient;
		_lunchDataFiler = lunchDataFiler;
		_lunchDataHelper = lunchDataHelper;
		_logger = logger;
		_messageAuthor = messageAuthor;
		_chatHandler = chatHandler;
		_appDataFiler = appDataFiler;
		_minMessageCount = configurationRoot.GetValue<int>("RemindMinimumMessageCount");
	}

	protected override async Task<int> OnExecute(CommandLineApplication app)
	{
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

		if (string.IsNullOrWhiteSpace(LunchDataPath))
		{
			if (!_lunchDataHelper.TryPromptForLunchData(out string lunchDataPath))
			{
				_logger.Information("Exiting");
				return await CommandHelper.ExecuteRootCommand(app);
			}

			LunchDataPath = lunchDataPath;
		}

		LunchData lunchData = await _lunchDataFiler.Load(LunchDataPath);

		User conductor = await _graphServiceClient.Me.Request().GetAsync();

		try
		{
			AppData appData = await _appDataFiler.Load();
			Assert.ConductorIsExpected(conductor, appData.ConductorDisplayName);
		}
		catch (Exception e)
		{
			_logger.Information(e, "Exiting");
			return await CommandHelper.ExecuteRootCommand(app);
		}

		if (lunchData.WasDryRun)
		{
			Console.WriteLine("FYI this lunch was a dry run");
		}

		if (!lunchData.Successful)
		{
			Console.WriteLine("FYI this lunch was not successful");
		}

		IReadOnlyList<string> silentChatIds = await FindSilentChats(lunchData, conductor);

		if (!silentChatIds.Any())
		{
			_logger.Information("There are no silent chats");
			return await CommandHelper.ExecuteRootCommand(app);
		}

		await DisplaySilentChats(silentChatIds, conductor.DisplayName);

		if (!Blocker.RequestUserCodeVerification("Enter code to send reminders"))
		{
			_logger.Warning("Incorrect code entered. Cancelling reminder");
			return await CommandHelper.ExecuteRootCommand(app);
		}

		await SendReminders(silentChatIds);

		_logger.Information("Finished sending reminder messages");

		return await CommandHelper.ExecuteRootCommand(app);
	}

	private async Task DisplaySilentChats(IReadOnlyList<string> silentChatIds, string conductorDisplayName)
	{
		StringBuilder stringBuilder = new("Chats with these members haven't messaged yet:\n");

		for (int i = 0; i < silentChatIds.Count; i++)
		{
			string chatId = silentChatIds[i];

			IChatMembersCollectionPage? members;

			try
			{
				members = await _graphServiceClient.Chats[chatId].Members.Request().GetAsync();
			}
			catch (Exception e)
			{
				_logger.Error(e, $"Could not find chat members: {chatId}");
				continue;
			}

			IEnumerable<string> usersNames = members.CurrentPage
				.Where(m => m.DisplayName != conductorDisplayName)
				.Select(m => m.DisplayName.Split(" ").First())
				.OrderBy(n => n);
			string membersStr = string.Join(", ", usersNames);

			stringBuilder.AppendLine($"\t{i}: {membersStr}");
		}

		Console.WriteLine(stringBuilder);
	}

	private async Task SendReminders(IEnumerable<string> chatIds)
	{
		int failedCount = 0;

		foreach (string chatId in chatIds)
		{
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

	private async Task<IReadOnlyList<string>> FindSilentChats(LunchData lunchData, User conductor)
	{
		List<string> silentChatIds = new();

		ProgressBarOptions options = new()
		{
			ForegroundColor = ConsoleColor.White,
			DisplayTimeInRealTime = true
		};

		using ProgressBar progressBar = new(lunchData.Chats.Count, "Searching", options);
		IProgress<double> progress = progressBar.AsProgress<double>();

		for (int i = 0; i < lunchData.Chats.Count; i++)
		{
			string chatId = lunchData.Chats[i].ChatId;

			(bool success, int messageCount) = await GetUserMessageCount(chatId, conductor);

			if (!success)
			{
				_logger.Error($"Failed to get user message count for {chatId}");
			}
			else if (messageCount <= _minMessageCount)
			{
				silentChatIds.Add(chatId);
			}

			progress.Report((double)i / (lunchData.Chats.Count - 1));
		}

		return silentChatIds;
	}

	private async Task<bool> SendReminder(string chatId)
	{
		string message = _messageAuthor.CreateReminderMessage();

		bool success = await _chatHandler.TrySendMessage(IsDryRun, chatId, message);
		return success;
	}

	private async Task<(bool success, int messageCount)> GetUserMessageCount(string chatId, User conductor)
	{
		IChatMessagesCollectionPage messages;

		try
		{
			messages = await _graphServiceClient.Chats[chatId].Messages.Request().GetAsync();
		}
		catch (Exception e)
		{
			_logger.Error(e, $"Could not find chat messages: {chatId}");
			return (false, 0);
		}

		bool IsNotEvent(ChatMessage m) => m.EventDetail is null;
		bool IsNotApplication(ChatMessage m) => m.From.Application is null;
		bool IsFromUser(ChatMessage m) => m.From.User?.Id != conductor.Id;

		int userMessageCount = messages.Where(IsNotEvent).Where(IsNotApplication).Count(IsFromUser);
		return (true, userMessageCount);
	}
}