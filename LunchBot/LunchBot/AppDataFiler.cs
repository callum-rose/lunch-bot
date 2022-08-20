using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace LunchBot;

public class AppDataFiler
{
    public readonly string Path;
    
    private const string Extension = ".appdata";

    private readonly string _directory;

    private readonly ILogger _logger;

    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Include, 
        DefaultValueHandling = DefaultValueHandling.Include
    };


    public AppDataFiler(IConfigurationRoot configuration, ILogger logger)
    {
        _logger = logger;
        _directory = configuration.GetValue<string>("OutputDirectory");
        Path = System.IO.Path.Combine(_directory, Extension);
    }

    public async Task<AppData> Load()
    {
        if (!File.Exists(Path))
        {
            return await CreateAndSaveNew();
        }

        string json = await File.ReadAllTextAsync(Path);

        AppData appData;

        try
        {
            appData = JsonConvert.DeserializeObject<AppData>(json, _settings);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Failed to deserialise file at path {Path}");
            return null;
        }

        _logger.Information($"Loaded {Extension} at {Path}");

        return appData;
    }

    public async Task<AppData> CreateAndSaveNew()
    {
        Directory.CreateDirectory(_directory);
     
        if (File.Exists(Path))
        {
            _logger.Information($"Already created app data file at {Path}");
            return await Load();
        }
        
        AppData appData = new();

        string json = JsonConvert.SerializeObject(appData, _settings);

        try
        {
            await File.WriteAllTextAsync(Path, json);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Failed to write file to {Path}");
            return null;
        }

        _logger.Warning($"Created {Path}. Now open to populate data");
        return appData;
    }
}