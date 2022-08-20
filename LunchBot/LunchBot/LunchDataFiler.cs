using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using File = System.IO.File;

namespace LunchBot;

public class LunchDataFiler
{
    public const string Extension = ".lunchdata";
    public readonly string Directory;

    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented
    };

    public LunchDataFiler(IConfigurationRoot configuration)
    {
        Directory = configuration.GetValue<string>("OutputDirectory");
    }

    public async Task<LunchData> Load(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        string json = await File.ReadAllTextAsync(path);
        return JsonConvert.DeserializeObject<LunchData>(json);
    }

    public async Task<IEnumerable<LunchData>> LoadAll(bool includeDryRuns = false)
    {
        List<LunchData> lunchDatas = new();

        foreach (string path in System.IO.Directory.EnumerateFiles(Directory).Where(p => Path.GetExtension(p) == Extension))
        {
            LunchData lunchData = await Load(path);

            if (lunchData is null)
            {
                continue;
            }

            if (lunchData.Successful && (!lunchData.WasDryRun || includeDryRuns))
            {
                lunchDatas.Add(lunchData);
            }
        }

        return lunchDatas;
    }

    public async Task Save(string name, LunchData lunchData)
    {
        System.IO.Directory.CreateDirectory(Directory);

        string json = JsonConvert.SerializeObject(lunchData, _settings);

        string GetPath(int? number)
        {
            string dryRun = lunchData.WasDryRun ? "_dryRun" : "";
            string num = number.HasValue ? $"_{number.Value}" : "";
            string fileName = $"{name}{dryRun}{num}{Extension}";
            return Path.Combine(Directory, fileName);
        }

        string path = GetPath(null);

        for (int i = 1; File.Exists(path); i++)
        {
            path = GetPath(i);
        }

        await File.WriteAllTextAsync(path!, json);
    }
}