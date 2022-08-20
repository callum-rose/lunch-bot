using System.Diagnostics;
using Serilog;

namespace LunchBot;

internal class LogWatch : IDisposable
{
    public long ElapsedSeconds => _stopwatch.ElapsedMilliseconds / 1000;
    
    private readonly string _name;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;

    private LogWatch(string name, ILogger logger)
    {
        _name = name;
        _logger = logger;
        _stopwatch = new Stopwatch();
    }

    public static LogWatch Start(string name, ILogger logger)
    {
        LogWatch logWatch = new(name, logger);
        logWatch._stopwatch.Start();
        return logWatch;
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.Information($"{_name}: {_stopwatch.ElapsedMilliseconds}ms");
    }
}