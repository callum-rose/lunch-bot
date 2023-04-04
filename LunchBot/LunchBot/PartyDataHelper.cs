using Serilog;

namespace LunchBot;

public class PartyDataHelper
{
    private readonly PartyDataFiler _partyDataFiler;
    private readonly ILogger _logger;

    public PartyDataHelper(PartyDataFiler partyDataFiler, ILogger logger)
    {
        _partyDataFiler = partyDataFiler;
        _logger = logger;
    }
    
    public bool TryPromptForPartyData(out string partyDataPath)
    {
        string[] paths = Directory.EnumerateFiles(_partyDataFiler.Directory)
            .Where(p => Path.GetExtension(p) == PartyDataFiler.Extension)
            .OrderByDescending(File.GetCreationTime)
            .ToArray();

        if (paths.Length == 0)
        {
            _logger.Error($"Could not find any {PartyDataFiler.Extension} files in \"{_partyDataFiler.Directory}\"");
            partyDataPath = string.Empty;
            return false;
        }

        Console.WriteLine("Ordered by date created:");

        for (int i = 0; i < paths.Length; i++)
        {
            string path = paths[i];
            Console.WriteLine($"{i}: {path}");
        }

        string input = Console.ReadLine();

        if (!int.TryParse(input, out int index))
        {
            _logger.Error($"Could not parse input {input} as an integer");
            partyDataPath = string.Empty;
            return false;
        }

        if (index < 0 || index >= paths.Length)
        {
            _logger.Error("Index out of range");
            partyDataPath = string.Empty;
            return false;
        }

        partyDataPath = paths[index];
        return true;
    }
}