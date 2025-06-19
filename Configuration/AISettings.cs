namespace AIConsoleApp.Configuration;

public interface IAISettings
{
    string ApiKey { get; }
    string Model { get; }
    string Url { get; }
}

public class AISettings : IAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-3.5-turbo";
    public string Url { get; set; } = "https://api.openai.com/v1/chat/completions";
} 