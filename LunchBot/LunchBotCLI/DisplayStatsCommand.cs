using LunchBot;
using McMaster.Extensions.CommandLineUtils;

namespace LunchBotCLI;

[Command(Name = "stats", Description = "Show stats for the system",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
internal class DisplayStatsCommand : CommandBase
{
    [Option(ShortName = "d")] private bool IncludeDryRuns { get; set; }

    private readonly UserMatrixHandler _userMatrixHandler;
    private readonly PartyDataFiler _partyDataFiler;
    private readonly PartyDataHelper _partyDataHelper;
    private readonly LunchDataFiler _lunchDataFiler;

    public DisplayStatsCommand(UserMatrixHandler userMatrixHandler, PartyDataFiler partyDataFiler,
        PartyDataHelper partyDataHelper, LunchDataFiler lunchDataFiler)
    {
        _userMatrixHandler = userMatrixHandler;
        _partyDataFiler = partyDataFiler;
        _partyDataHelper = partyDataHelper;
        _lunchDataFiler = lunchDataFiler;
    }

    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        UserMatrix cumulative = await _userMatrixHandler.GetCumulative(IncludeDryRuns);

        PartyData upcomingPartyData = null;

        if (Prompt.GetYesNo("Include data for upcoming party?", true))
        {
            if (_partyDataHelper.TryPromptForPartyData(out string partyDataPath))
            {
                upcomingPartyData = await _partyDataFiler.Load(partyDataPath);
                cumulative.Add(upcomingPartyData!.Party);
            }
        }

        await DisplayOptimalMeetCount(upcomingPartyData);

        DisplayMeetCountDistributions(cumulative);

        return await CommandHelper.ExecuteRootCommand(app);
    }

    private async Task DisplayOptimalMeetCount(PartyData upcomingPartyData)
    {
        int GetMeetCount(int groupSize) => groupSize * (groupSize - 1) / 2;

        int optimalMeetCount = 0;

        foreach (LunchData lunchData in await _lunchDataFiler.LoadAll())
        {
            foreach (GroupChat chat in lunchData.Chats)
            {
                optimalMeetCount += GetMeetCount(chat.Users.Count);
            }
        }

        if (upcomingPartyData is not null)
        {
            foreach (Group group in upcomingPartyData.Party.Groups)
            {
                optimalMeetCount += GetMeetCount(group.Users.Count);
            }
        }

        Console.WriteLine($"Optimal Meet Count: {optimalMeetCount}");
    }

    private static void DisplayMeetCountDistributions(UserMatrix cumulative)
    {
        Dictionary<int, int> meetCountDistributions = new();
        
        foreach ((int x, int y) in cumulative.Matrix.GetIterator())
        {
            int meetCount = cumulative.Matrix[x, y];

            if (!meetCountDistributions.ContainsKey(meetCount))
            {
                meetCountDistributions.Add(meetCount, 0);
            }

            meetCountDistributions[meetCount]++;
        }

        Console.WriteLine("Meet distributions:");

        foreach (KeyValuePair<int, int> pair in meetCountDistributions)
        {
            Console.WriteLine($"\t{pair.Key}: {pair.Value}");
        }
    }
}