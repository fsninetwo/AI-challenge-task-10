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
    private readonly string _path;
    private bool _headerWritten;

    public SampleOutputLogger(IConfiguration config)
    {
        _path = config["Logging:SampleOutputPath"] ?? "sample_outputs.md";
        _headerWritten = File.Exists(_path); // if file exists assume header was written
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

        AppendToFile(sb.ToString());
    }

    public void RecordNoMatch(string request) => Record(request, Enumerable.Empty<Product>());

    public void Flush()
    {
        // nothing to flush; records already written
    }

    private void AppendToFile(string run)
    {
        var content = new StringBuilder();
        if (!_headerWritten)
        {
            content.AppendLine("# Sample Outputs\n");
            _headerWritten = true;
        }

        content.AppendLine($"_Last Update: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC_\n");

        content.AppendLine(run);
        content.AppendLine();

        File.AppendAllText(_path, content.ToString());
        Console.WriteLine($"Saved sample output to {_path}\n");
    }
} 