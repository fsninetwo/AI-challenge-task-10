using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using AIConsoleApp.Configuration;
using AIConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace AIConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var serviceProvider = ConfigureServices();
        var app = serviceProvider.GetRequiredService<AIConsoleApp.App.ProductSearchConsoleApp>();
        await app.RunAsync();
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

        services.AddTransient<IProductSearch, ProductSearchService>();
        services.AddSingleton<AIConsoleApp.Logging.ISampleOutputLogger, AIConsoleApp.Logging.SampleOutputLogger>();
        services.AddTransient<AIConsoleApp.App.ProductSearchConsoleApp>();

        return services.BuildServiceProvider();
    }
} 