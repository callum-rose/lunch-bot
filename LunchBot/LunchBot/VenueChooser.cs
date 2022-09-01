using Microsoft.Extensions.Configuration;
using Serilog;

namespace LunchBot;

public class VenueChooser
{
    private readonly AppDataFiler _appDataFiler;
    private readonly ILogger _logger;

    private string[] _venues;

    public VenueChooser(AppDataFiler appDataFiler, ILogger logger)
    {
        _appDataFiler = appDataFiler;
        _logger = logger;
    }

    public async Task Initialise(int seed)
    {
        if (_venues is not null)
        {
            return;
        }

        AppData appData = await _appDataFiler.Load();
        _venues = appData.Venues
            .Shuffle(seed)
            .ToArray();

        _logger.Information($"{nameof(VenueChooser)} initialised with seed: {seed}");
    }

    public string GetVenue(int groupIndex)
    {
        if (_venues is null)
        {
            throw new Exception($"{nameof(VenueChooser)} not initialised");
        }

        return _venues[groupIndex % _venues.Length];
    }
}