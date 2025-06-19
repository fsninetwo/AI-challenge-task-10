using System.Text;
using AIConsoleApp.Models;

namespace AIConsoleApp.Logging;

public class SampleOutputLogger : ISampleOutputLogger
{
    private readonly List<string> _runs = new();

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
                sb.AppendLine($"{index++}. {p.Name} - ${p.Price:F2}, Rating: {p.Rating:F1}, {(p.InStock ? "In Stock" : "Out of Stock")}");
            }
        }
        else
        {
            sb.AppendLine("No products matched your criteria.");
        }

        _runs.Add(sb.ToString());
    }

    public void RecordNoMatch(string request) => Record(request, Array.Empty<Product>());

    public void Flush()
    {
        if (_runs.Count < 2) return;
        var md = new StringBuilder();
        md.AppendLine("# Sample Outputs\n");
        foreach (var run in _runs)
        {
            md.AppendLine(run);
            md.AppendLine();
        }
        File.WriteAllText("sample_outputs.md", md.ToString());
        Console.WriteLine("Sample outputs written to sample_outputs.md\n");
    }
} 