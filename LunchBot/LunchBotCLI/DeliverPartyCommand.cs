using System.Globalization;
using LunchBot;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Graph;
using Serilog;
using Prompt = McMaster.Extensions.CommandLineUtils.Prompt;

namespace LunchBotCLI;

[Command(Name = "deliver",
    Description = "Deliver the lunch from a generated party",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
internal class DeliverPartyCommand : CommandBase
{
    [Option(CommandOptionType.SingleOrNoValue, ShortName = "d", Description = "Is this a test run")]
    private bool IsDryRun { get; set; } = true;

    [Option(ShortName = "p", Description = "The absolute path to the party data file to use")]
    private string PartyDataPath { get; set; } = "";

    [Option(ShortName = "n", Description = "Optional name override for the lunch instead of the current month")]
    private string PartyName { get; set; }

    private readonly ILogger _logger;
    private readonly AppDataFiler _appDataFiler;
    private readonly UserFinder _userFinder;
    private readonly UserMatrixHandler _userMatrixHandler;
    private readonly ChatOrchestrator _chatOrchestrator;
    private readonly LunchDataFiler _lunchDataFiler;
    private readonly PartyDataFiler _partyDataFiler;
    private readonly TitleAuthor _titleAuthor;
    private readonly MessageAuthor _messageAuthor;
    private readonly PartyDataHelper _partyDataHelper;
    private readonly PartyDataDisplayer _partyDataDisplayer;

    public DeliverPartyCommand(ILogger logger, AppDataFiler appDataFiler, UserFinder userFinder,
        UserMatrixHandler userMatrixHandler,
        ChatOrchestrator chatOrchestrator, LunchDataFiler lunchDataFiler, PartyDataFiler partyDataFiler,
        TitleAuthor titleAuthor, MessageAuthor messageAuthor, PartyDataHelper partyDataHelper,
        PartyDataDisplayer partyDataDisplayer)
    {
        _logger = logger;
        _appDataFiler = appDataFiler;
        _userFinder = userFinder;
        _userMatrixHandler = userMatrixHandler;
        _chatOrchestrator = chatOrchestrator;
        _lunchDataFiler = lunchDataFiler;
        _partyDataFiler = partyDataFiler;
        _titleAuthor = titleAuthor;
        _messageAuthor = messageAuthor;
        _partyDataHelper = partyDataHelper;
        _partyDataDisplayer = partyDataDisplayer;
    }

    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        _logger.Information($"Starting delivery command. {nameof(IsDryRun)}: {IsDryRun}");

        if (string.IsNullOrEmpty(PartyDataPath))
        {
            if (!_partyDataHelper.TryPromptForPartyData(out string partyDataPath))
            {
                _logger.Information("Exiting");
                return await CommandHelper.ExecuteRootCommand(app);
            }

            PartyDataPath = partyDataPath;
        }

        if (string.IsNullOrWhiteSpace(PartyName))
        {
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Today);
            PartyName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month);
        }

        PartyData partyData = await _partyDataFiler.Load(PartyDataPath);

        if (partyData is null)
        {
            _logger.Information("Exiting");
            return await CommandHelper.ExecuteRootCommand(app);
        }

        User conductor = await _userFinder.GetConductingUser();

        _logger.Information($"Got conductor: {conductor?.DisplayName}");

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

        Console.WriteLine("Party Data:");

        _partyDataDisplayer.DisplayData(partyData);

        string demoTitle = _titleAuthor.GetTitle(PartyName!);
        string demoMessage = _messageAuthor.CreateTestInitialChatMessage();

        Console.WriteLine($"Demo Title: \"{demoTitle}\"");
        Console.WriteLine($"Demo Message: \"{demoMessage}\"");

        bool textOkay = Prompt.GetYesNo("Is the title and message okay?", false);

        if (!textOkay)
        {
            Console.WriteLine("Go to appsettings.json to update the content");
            return await CommandHelper.ExecuteRootCommand(app);
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

        LunchData lunchData = await _chatOrchestrator.DeliverAll(IsDryRun, 0, conductor!, PartyName!, partyData);
        await _lunchDataFiler.Save(PartyName!, lunchData);

        if (lunchData.Successful)
        {
            _logger.Information("Successfully delivered all messages");
        }
        else
        {
            _logger.Error("An error occured when trying to deliver messages");
            return await CommandHelper.ExecuteRootCommand(app);
        }

        await _userMatrixHandler.AddAndSaveCumulative(partyData.Party);

        return await CommandHelper.ExecuteRootCommand(app);
    }
}