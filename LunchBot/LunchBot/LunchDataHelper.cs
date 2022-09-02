using Serilog;

namespace LunchBot;

public class LunchDataHelper
{
    private readonly LunchDataFiler _lunchDataFiler;
    private readonly ILogger _logger;

    public LunchDataHelper(LunchDataFiler lunchDataFiler, ILogger logger)
    {
        _lunchDataFiler = lunchDataFiler;
        _logger = logger;
    }
    
    public bool TryPromptForLunchData(out string partyDataPath)
    {
        string[] paths = Directory.EnumerateFiles(_lunchDataFiler.Directory)
            .Where(p => Path.GetExtension(p) == LunchDataFiler.Extension)
            .OrderByDescending(File.GetCreationTime)
            .ToArray();

        if (paths.Length == 0)
        {
            _logger.Error($"Could not find any {LunchDataFiler.Extension} files in \"{_lunchDataFiler.Directory}\"");
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
            partyDataPath = string.Empty;
            return false;
        }

        partyDataPath = paths[index];
        return true;
    }
}