using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.Services;

public abstract class LoggingBase<T> where T : class
{
    protected readonly ILogger<T> Logger;

    protected LoggingBase(ILogger<T> logger)
    {
        Logger = logger;
    }
}