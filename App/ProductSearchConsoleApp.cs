using AIConsoleApp.Logging;
using AIConsoleApp.Services;
using System;
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
        Console.WriteLine("\n=== Welcome to the AI Product Finder ===");
        Console.WriteLine("Describe what you are looking for in natural language (e.g. 'Smartphone under $800 with a great camera').");
        Console.WriteLine("Type 'exit' anytime to quit.\n");
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Please enter a description of the product you need or type 'exit' to leave.\n");
                continue;
            }

            Console.WriteLine("Searching, please wait...\n");

            List<Product> products;
            try
            {
                products = (await _search.SearchAsync(input)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sorry, something went wrong while contacting the AI service: {ex.Message}\n");
                continue;
            }

            if (products.Any())
            {
                Console.WriteLine($"Found {products.Count} product{(products.Count > 1 ? "s" : string.Empty)}:\n");
                Console.WriteLine("Filtered Products:");
                var index = 1;
                foreach (var p in products)
                {
                    Console.WriteLine($"{index++,2}. {p.Name} - ${p.Price,8:F2}, Rating: {p.Rating,4:F1}, {(p.InStock ? "In Stock" : "Out of Stock")} ");
                }
                Console.WriteLine();
                _logger.Record(input!, products);
            }
            else
            {
                Console.WriteLine("Unfortunately, no products matched your criteria. Try refining your request or relaxing some constraints.\n");
                _logger.RecordNoMatch(input!);
            }
        }

        _logger.Flush();

        Console.WriteLine("Thank you for using the AI Product Finder. Goodbye!\n");
    }
} 