using Microsoft.Extensions.Logging;

public abstract class ServiceBase<T> where T : class
{
    protected readonly ILogger<T> Logger;

    protected ServiceBase(ILogger<T> logger)
    {
        Logger = logger;
    }
}