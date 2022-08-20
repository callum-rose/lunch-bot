using LunchBot;
using McMaster.Extensions.CommandLineUtils;

namespace LunchBotCLI;

[Command(Name = "partydatapaths", Description = "Display the paths of all the party data files",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
internal class DisplayPartyDataCommand : CommandBase
{
    private readonly PartyDataFiler _partyDataFiler;

    public DisplayPartyDataCommand(PartyDataFiler partyDataFiler)
    {
        _partyDataFiler = partyDataFiler;
    }

    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        Console.WriteLine("Ordered by date created:");

        int i = 1;

        foreach (string path in Directory.EnumerateFiles(_partyDataFiler.Directory)
                     .Where(p => Path.GetExtension(p) == PartyDataFiler.Extension)
                     .OrderByDescending(File.GetCreationTime))
        {
            Console.WriteLine($"{i++}: {path}");
        }

        return await CommandHelper.ExecuteRootCommand(app);
    }
}