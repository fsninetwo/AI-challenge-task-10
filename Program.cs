using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using AIConsoleApp.Configuration;
using AIConsoleApp.Services;

namespace AIConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var serviceProvider = ConfigureServices();
        var processor = serviceProvider.GetRequiredService<ITextProcessor>();

        Console.WriteLine("Type your prompt (or 'exit' to quit):");
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                break;

            var result = await processor.ProcessAsync(input ?? string.Empty);
            Console.WriteLine($"AI: {result}\n");
        }
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();

        services.AddSingleton<IAISettings>(_ =>
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Warning: OPENAI_API_KEY environment variable not set. Requests will fail without it.");
            }

            return new AISettings
            {
                ApiKey = apiKey ?? string.Empty,
                Model = "gpt-3.5-turbo"
            };
        });

        services.AddTransient<ITextProcessor, OpenAITextProcessor>();

        return services.BuildServiceProvider();
    }
} 