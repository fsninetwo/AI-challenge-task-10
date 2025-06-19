namespace AIConsoleApp.Configuration;

public interface IAISettings
{
    string ApiKey { get; }
    string Model { get; }
    string Url { get; }
}

public class AISettings : IAISettings
{
    public string ApiKey { get; set; }
    public string Model { get; set; }
    public string Url { get; set; }
} 