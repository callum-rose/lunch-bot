using McMaster.Extensions.CommandLineUtils;

namespace LunchBotCLI;

internal static class CommandHelper
{
    public static Task<int> ExecuteRootCommand(CommandLineApplication app, bool waitForKey = true)
    {
        Console.WriteLine();
        
        if (waitForKey)
        {
            Console.WriteLine("Press any key to return to root command...");
            Console.ReadKey(true);
            Console.WriteLine();
        }
        
        while (app.Parent is not null)
        {
            app = app.Parent;
        }

        // TODO This is a bit brittle, unsure if a better way to return to main
        return app.ExecuteAsync(new[] { "LunchBot" });
    }
}