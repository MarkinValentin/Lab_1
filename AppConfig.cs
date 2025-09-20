using System.Text.Json;

public sealed class AppConfig
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5432;
    public string Database { get; init; } = "appdb";
    public string SslMode { get; init; } = "Disable";
    public int TimeoutSeconds { get; init; } = 5;
    public int CommandTimeoutSeconds { get; init; } = 10;

    public static AppConfig Load(string path = "appsettings.json")
    {
        var json = File.ReadAllText(path);
        var cfg = JsonSerializer.Deserialize<AppConfig>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (cfg is null) throw new InvalidOperationException("Bad config");
        return cfg;
    }
}


