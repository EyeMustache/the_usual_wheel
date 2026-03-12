using System.IO;

namespace TheUsualWheelProject.Services;

public static class DatabaseConfig
{
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, "theusualwheel.db");

    public static string ConnectionString =>
        $"Data Source={DatabasePath}";
}