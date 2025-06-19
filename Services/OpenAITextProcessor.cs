using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AIConsoleApp.Configuration;
using System.Threading.Tasks;

namespace AIConsoleApp.Services;

public class OpenAITextProcessor : ITextProcessor
{
    private readonly HttpClient _httpClient;
    private readonly IAISettings _settings;

    public OpenAITextProcessor(IHttpClientFactory factory, IAISettings settings)
    {
        _httpClient = factory.CreateClient();
        _settings = settings;
    }

    public async Task<string> ProcessAsync(string input)
    {
        const string url = "https://api.openai.com/v1/chat/completions";

        var requestBody = new
        {
            model = _settings.Model,
            messages = new[] { new { role = "user", content = input } }
        };
        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(responseContent);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content?.Trim() ?? string.Empty;
    }
} 