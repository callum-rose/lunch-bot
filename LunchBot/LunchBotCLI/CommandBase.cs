using McMaster.Extensions.CommandLineUtils;

namespace LunchBotCLI;

[HelpOption("--help")]
internal abstract class CommandBase
{
    protected virtual async Task<int> OnExecute(CommandLineApplication app)
    {
        app.ShowHelp();

        return await CommandHelper.ExecuteRootCommand(app, false);
    }
}