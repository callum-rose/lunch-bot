using McMaster.Extensions.CommandLineUtils;

namespace LunchBotCLI;

[Command(Name = "LunchBot", Description = "",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
[Subcommand(typeof(InitialSetupCommand), typeof(CreatePartyCommand), typeof(DeliverPartyCommand),
    typeof(DisplayPartyDataCommand), typeof(DisplayStatsCommand), typeof(RemindGroupsCommand))]
internal class MainCommand : CommandBase
{
    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        // TODO if you ask for help the application quits, unsure how to hijack the help method atm
        app.ShowHelp();

        string command = Prompt.GetString("Run a command:");

        if (string.IsNullOrEmpty(command))
        {
            return 0;
        }

        string[] args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return await app.ExecuteAsync(args);
    }
}