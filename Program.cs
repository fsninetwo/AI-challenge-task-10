using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using AIConsoleApp.Configuration;
using AIConsoleApp.Services;
using Microsoft.Extensions.Configuration;

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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddHttpClient();

        services.AddSingleton<IAISettings>(_ =>
        {
            var openAi = configuration.GetSection("OpenAI");

            var apiKey = configuration["OPENAI_API_KEY"] ?? openAi["ApiKey"];
            var model = openAi["Model"] ?? "gpt-3.5-turbo";
            var url = openAi["Url"] ?? "https://api.openai.com/v1/chat/completions";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Warning: OPENAI_API_KEY environment variable or OpenAI:ApiKey not set. Requests will fail without it.");
            }

            return new AISettings
            {
                ApiKey = apiKey ?? string.Empty,
                Model = model,
                Url = url
            };
        });

        services.AddTransient<ITextProcessor, OpenAITextProcessor>();

        return services.BuildServiceProvider();
    }
} 