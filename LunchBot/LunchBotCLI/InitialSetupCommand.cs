using LunchBot;
using McMaster.Extensions.CommandLineUtils;
using Serilog;

namespace LunchBotCLI;

[Command(Name = "setup", Description = "Create the .appdata config file",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
internal class InitialSetupCommand : CommandBase
{
    private readonly AppDataFiler _appDataFiler;
    private readonly ILogger _logger;

    public InitialSetupCommand(AppDataFiler appDataFiler, ILogger logger)
    {
        _appDataFiler = appDataFiler;
        _logger = logger;
    }
    
    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        await _appDataFiler.CreateAndSaveNew();
        
        _logger.Information($"Open file at {_appDataFiler.Path} and populate the data");
        
        return await CommandHelper.ExecuteRootCommand(app);
    }
}