using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using AIConsoleApp.Models;
using Microsoft.Extensions.Configuration;

namespace AIConsoleApp.Logging;

public class SampleOutputLogger : ISampleOutputLogger
{
    private readonly List<string> _runs = new();
    private readonly string _path;

    public SampleOutputLogger(IConfiguration config)
    {
        _path = config["Logging:SampleOutputPath"] ?? "sample_outputs.md";
    }

    public void Record(string request, IEnumerable<Product> products)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"### Request: {request}");
        if (products.Any())
        {
            sb.AppendLine("Filtered Products:");
            var index = 1;
            foreach (var p in products)
            {
                sb.AppendLine($"{index++,2}. {p.Name} - ${p.Price,8:F2}, Rating: {p.Rating,4:F1}, {(p.InStock ? "In Stock" : "Out of Stock")}");
            }
        }
        else
        {
            sb.AppendLine("No products matched your criteria.");
        }

        _runs.Add(sb.ToString());
    }

    public void RecordNoMatch(string request) => Record(request, Enumerable.Empty<Product>());

    public void Flush()
    {
        if (_runs.Count == 0) return;

        var content = new StringBuilder();

        // If file doesn't exist, write header first
        if (!File.Exists(_path))
        {
            content.AppendLine("# Sample Outputs\n");
        }

        foreach (var run in _runs)
        {
            content.AppendLine(run);
            content.AppendLine();
        }

        // Append to existing file
        File.AppendAllText(_path, content.ToString());
        Console.WriteLine($"Appended {_runs.Count} sample output(s) to {_path}\n");
    }
} 