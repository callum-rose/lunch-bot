using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace LunchBot;

public class UserIndexerFiler
{
    private readonly ILogger _logger;
    private readonly string _path;

    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented
    };
    
    public UserIndexerFiler(IConfigurationRoot configuration, ILogger logger)
    {
        _logger = logger;
        
        string directory = configuration.GetValue<string>("OutputDirectory");
        string fileName = configuration.GetValue<string>("UserIdFileName");
        _path = Path.Combine(directory, fileName);
    }
    
    public async Task<UserIndexer> Load()
    {
        if (!File.Exists(_path))
        {
            _logger.Warning($"Can't load {nameof(UserIndexer)} from path {_path}. Creating new");
            return new UserIndexer();
        }
        
        string indexerJson = await File.ReadAllTextAsync(_path);
        UserIndexer userIndexer = JsonConvert.DeserializeObject<UserIndexer>(indexerJson);
        
        _logger.Information($"Loaded {nameof(UserIndexer)} from path {_path}");

        return userIndexer;
    }

    public async Task Save(UserIndexer indexer)
    {
        string directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);

        string json = JsonConvert.SerializeObject(indexer, _settings);
        await File.WriteAllTextAsync(_path, json);
        
        _logger.Information($"Saved {nameof(UserIndexer)} to path {_path}");
    }
}