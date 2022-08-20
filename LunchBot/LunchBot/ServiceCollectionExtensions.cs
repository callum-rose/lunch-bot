using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace LunchBot;

public static class ServiceCollectionExtensions
{
    public static void AddLunchBot(this IServiceCollection serviceCollection)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string logsDirectory = configuration.GetValue<string>("LogsDirectory");

        Logger logger = new LoggerConfiguration()
            .WriteTo.File(logsDirectory, LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .WriteTo.Console(LogEventLevel.Information)
            .CreateLogger();
        
        serviceCollection.AddLogging(config =>
        {
            config.ClearProviders();
            config.AddProvider(new SerilogLoggerProvider(logger));
        });
        
        serviceCollection
            .AddSingleton(_ => configuration)
            .AddSingleton<LoggerFactory>()
            .AddSingleton(provider => provider.GetService<LoggerFactory>()!.Create())
            .AddTransient<Serializer>()
            .AddSingleton<IAuthenticationData, EnvAuthenticationData>()
            .AddSingleton<HttpProvider>()
            .AddSingleton<GraphServiceClientFactory>()
            .AddSingleton(provider => provider.GetService<GraphServiceClientFactory>()!.Create())
            .AddSingleton<AppDataFiler>()
            .AddSingleton<UserFinder>()
            .AddSingleton<PeopleFileReader>()
            .AddSingleton<UserIndexerFiler>()
            .AddSingleton<UserIndexerHandler>()
            .AddSingleton<UserMatrixFiler>()
            .AddSingleton<UserMatrixHandler>()
            .AddSingleton<IGroupSizer, BasicGroupSizer>()
            .AddSingleton<IPartyScorer, LunchedAndDepartmentPartyScorer>()
            .AddSingleton<IGroupSelector, SwapGroupSelector>()
            .AddSingleton<PartyDataFiler>()
            .AddSingleton<LunchDataFiler>()
            .AddSingleton<TitleAuthor>()
            .AddSingleton<MessageAuthor>()
            .AddSingleton<VenueChooser>()
            .AddSingleton<ChatHandler>()
            .AddSingleton<ChatOrchestrator>()
            .AddSingleton<PartyGenerator>()
            .AddSingleton<PartyDataDisplayer>()
            .AddSingleton<LunchDataHelper>()
            .AddSingleton<PartyDataHelper>();
    }
}