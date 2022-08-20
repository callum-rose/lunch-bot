using LunchBot;
using LunchBotCLI;
using Microsoft.Extensions.Hosting;

Start:

IHostBuilder hostBuilder = new HostBuilder().ConfigureServices((_, services) => services.AddLunchBot());

try
{
    await hostBuilder.RunCommandLineApplicationAsync<MainCommand>(args);
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.WriteLine("Press R to restart, or any other key to close the console");
    
    ConsoleKeyInfo key = Console.ReadKey(true);
    
    if (key.KeyChar == 'r')
    {
        // I know goto is frowned upon but I think it makes sense here. The app as it stands will quit if you use --help
        // This allows the user to reset the app without closing the console
        goto Start;
    }
    
    return 1;
}

Console.WriteLine("Program ended. Press any key to close the console");
Console.ReadKey(true);

return 0;