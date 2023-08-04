using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace LunchBot;

public class PartyDataFiler
{
    public const string Extension = ".partydata";
    public readonly string Directory;

    private readonly ILogger _logger;
    
    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented
    };

    public PartyDataFiler(IConfigurationRoot configuration, ILogger logger)
    {
        _logger = logger;
        Directory = configuration.GetValue<string>("OutputDirectory");
    }

    public async Task<PartyData> Load(string path)
    {
        if (!File.Exists(path))
        {
            _logger.Error($"Can't load {Extension} file at \"{path}\"");
            return null;
        }

        string json = await File.ReadAllTextAsync(path);
        
        PartyData partyData;
        
        try
        {
            partyData = JsonConvert.DeserializeObject<PartyData>(json);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Failed to deserialise file at path {path}");
            return null;
        }
        
        _logger.Information($"Loaded {Extension} at {path}");

        return partyData;
    }

    public async Task Save(PartyData lunchData, string name)
    {
        System.IO.Directory.CreateDirectory(Directory);

        string json = JsonConvert.SerializeObject(lunchData, _settings);

        string GetPath(int? number)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string suffix = number.HasValue ? $"_{number.Value}" : "";
            string fileName = $"{name}_{date}{suffix}{Extension}";
            return Path.Combine(Directory, fileName);
        }

        string path = GetPath(null);

        for (int i = 1; File.Exists(path); i++)
        {
            path = GetPath(i);
        }

        try
        {
            await File.WriteAllTextAsync(path!, json);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Failed to write {Extension} file to {path}");
            return;
        }
        
        _logger.Information($"Saved {Extension} to {path}");
    }
}