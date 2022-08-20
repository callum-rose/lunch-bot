using Newtonsoft.Json;

namespace LunchBot;

public class AppData
{
    public string ConductorDisplayName { get; }
    public IReadOnlyDictionary<string, string> NameMappings { get; }
    public IReadOnlyList<string> Venues { get; }

    public AppData()
    {
        ConductorDisplayName = "";
        NameMappings = new Dictionary<string, string>
        {
            { "Kimothy, Test", "Kim, Test" }
        };
        Venues = Array.Empty<string>();
    }
    
    [JsonConstructor]
    private AppData(string conductorDisplayName, Dictionary<string, string> nameMappings, string[] venues)
    {
        ConductorDisplayName = conductorDisplayName;
        NameMappings = nameMappings;
        Venues = venues;
    }
}