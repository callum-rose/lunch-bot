using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace LunchBot;

internal class LoggerFactory : IDisposable
{
    private readonly string _path;

    private readonly List<Logger> _loggers = new();

    public LoggerFactory(IConfigurationRoot configuration)
    {
        _path = configuration.GetValue<string>("LogsDirectory");
    }

    public ILogger Create()
    {
        Logger logger = new LoggerConfiguration()
            .WriteTo.File(_path, LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .WriteTo.Console(LogEventLevel.Information)
            .CreateLogger();
        
        _loggers.Add(logger);
        
        return logger;
    }
    
    public void Dispose()
    {
        foreach (Logger logger in _loggers)
        {
            logger.Dispose();
        }
    }
}