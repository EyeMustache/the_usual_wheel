using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.Services;

public class JsonDataService : LoggingBase<JsonDataService>
{
    public JsonDataService(ILogger<JsonDataService> logger) : base(logger)
    {
        Logger.LogInformation("Initialized.");
    }

}