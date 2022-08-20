using Microsoft.Extensions.Configuration;
using Serilog;

namespace LunchBot;

public class VenueChooser
{
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _logger;

    private string[] _venues;

    public VenueChooser(IConfigurationRoot configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Initialise(int seed)
    {
        if (_venues is not null)
        {
            return;
        }
        
        _venues = _configuration.GetSection("VenuesToSuggest")
            .GetChildren()
            .Select(x => x.Value)
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