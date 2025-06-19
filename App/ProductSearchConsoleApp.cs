using AIConsoleApp.Logging;
using AIConsoleApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIConsoleApp.App;

public class ProductSearchConsoleApp
{
    private readonly IProductSearch _search;
    private readonly ISampleOutputLogger _logger;

    public ProductSearchConsoleApp(IProductSearch search, ISampleOutputLogger logger)
    {
        _search = search;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Type your prompt (or 'exit' to quit):");
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                break;

            var products = (await _search.SearchAsync(input ?? string.Empty)).ToList();

            if (products.Any())
            {
                Console.WriteLine("Filtered Products:");
                var index = 1;
                foreach (var p in products)
                {
                    Console.WriteLine($"{index++}. {p.Name} - ${p.Price:F2}, Rating: {p.Rating:F1}, {(p.InStock ? "In Stock" : "Out of Stock")}\n");
                }
                _logger.Record(input!, products);
            }
            else
            {
                Console.WriteLine("No products matched your criteria.\n");
                _logger.RecordNoMatch(input!);
            }
        }

        _logger.Flush();
    }
} 