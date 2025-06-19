using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using AIConsoleApp.Configuration;
using AIConsoleApp.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AIConsoleApp.Services;

public class ProductSearchService : IProductSearch
{
    private readonly HttpClient _httpClient;
    private readonly IAISettings _settings;
    private readonly List<Product> _products;

    public ProductSearchService(IHttpClientFactory factory, IAISettings settings)
    {
        _httpClient = factory.CreateClient();
        _settings = settings;
        _products = LoadProducts();
    }

    private static List<Product> LoadProducts()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "products.json");
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<Product>>(json, options) ?? new List<Product>();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string userQuery)
    {
        var requestBody = BuildChatRequest(userQuery);
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Url)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var criteria = ParseCriteria(jsonResponse);

        var results = ApplyFilter(criteria);
        return results;
    }

    private object BuildChatRequest(string userQuery)
    {
        // Build a short catalog summary to give the model basic knowledge of the dataset
        var categories = _products.Select(p => p.Category)
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .OrderBy(c => c)
                                   .ToList();

        var summary = $"The product catalog contains { _products.Count } items across these categories: " + string.Join(", ", categories) + ".";

        var functionDef = new
        {
            name = "filter_products",
            description = "Extract user product preferences for searching a product catalog",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    category = new { type = "string", description = "Desired product category" },
                    max_price = new { type = "number", description = "Maximum acceptable price" },
                    min_rating = new { type = "number", description = "Minimum star rating" },
                    in_stock = new { type = "boolean", description = "Whether the item must be in stock" },
                    keywords = new { type = "string", description = "Important keywords to match in the product name" }
                },
                required = Array.Empty<string>()
            }
        };

        return new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "system", content = summary },
                new { role = "user", content = userQuery }
            },
            functions = new[] { functionDef },
            function_call = "auto"
        };
    }

    private static FilterCriteria ParseCriteria(string responseJson)
    {
        var doc = JsonDocument.Parse(responseJson);
        var choice = doc.RootElement.GetProperty("choices")[0].GetProperty("message");
        if (!choice.TryGetProperty("function_call", out var funcCall))
        {
            // fallback empty criteria
            return new FilterCriteria();
        }

        var argumentsStr = funcCall.GetProperty("arguments").GetString() ?? "{}";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        try
        {
            return JsonSerializer.Deserialize<FilterCriteria>(argumentsStr, options) ?? new FilterCriteria();
        }
        catch
        {
            return new FilterCriteria();
        }
    }

    private IEnumerable<Product> ApplyFilter(FilterCriteria c)
    {
        IEnumerable<Product> query = _products;

        if (!string.IsNullOrWhiteSpace(c.Category))
        {
            query = query.Where(p => string.Equals(p.Category, c.Category, StringComparison.OrdinalIgnoreCase));
        }
        if (c.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= c.MaxPrice.Value);
        }
        if (c.MinRating.HasValue)
        {
            query = query.Where(p => p.Rating >= c.MinRating.Value);
        }
        if (c.InStock.HasValue)
        {
            if (c.InStock.Value)
                query = query.Where(p => p.InStock);
        }
        if (!string.IsNullOrWhiteSpace(c.Keywords))
        {
            var keywords = c.Keywords.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var kw in keywords)
            {
                query = query.Where(p => p.Name.Contains(kw, StringComparison.OrdinalIgnoreCase));
            }
        }

        return query;
    }

    private static bool IsEmpty(FilterCriteria c) =>
        string.IsNullOrWhiteSpace(c.Category) &&
        c.MaxPrice is null &&
        c.MinRating is null &&
        c.InStock is null &&
        string.IsNullOrWhiteSpace(c.Keywords);

    private FilterCriteria LocalParse(string userQuery)
    {
        var criteria = new FilterCriteria();
        var lower = userQuery.ToLowerInvariant();

        // price
        var priceMatch = Regex.Match(lower, @"(?:under|below|less than)\s*\$?\s*(\d+(?:\.\d+)?)");
        if (priceMatch.Success && decimal.TryParse(priceMatch.Groups[1].Value, out var price))
            criteria.MaxPrice = price;

        // rating
        var ratingMatch = Regex.Match(lower, @"rating (?:above|over|greater than|>)\s*(\d+(?:\.\d+)?)");
        if (ratingMatch.Success && double.TryParse(ratingMatch.Groups[1].Value, out var rating))
            criteria.MinRating = rating;

        // stock
        if (lower.Contains("in stock")) criteria.InStock = true;
        else if (lower.Contains("out of stock")) criteria.InStock = false;

        // category
        var allCategories = _products.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var cat in allCategories)
        {
            if (lower.Contains(cat.ToLowerInvariant())) { criteria.Category = cat; break; }
        }

        // keywords
        var stop = new HashSet<string>(new[] { "under", "below", "less", "than", "price", "rating", "above", "over", "greater", "in", "stock", "out", "of", "and", "with", "the", "a", "an", "for", "max", "minimum", "budget" });
        var words = Regex.Matches(lower, @"[a-zA-Z0-9\-']+").Select(m => m.Value).Where(w => w.Length > 2 && !stop.Contains(w) && !decimal.TryParse(w, out _));
        if (!string.IsNullOrWhiteSpace(criteria.Category)) words = words.Where(w => w != criteria.Category.ToLowerInvariant());
        criteria.Keywords = string.Join(' ', words);

        return criteria;
    }

    private static bool IsSameCriteria(FilterCriteria a, FilterCriteria b)
    {
        return (a.Category ?? string.Empty).Equals(b.Category ?? string.Empty, StringComparison.OrdinalIgnoreCase)
               && a.MaxPrice == b.MaxPrice && a.MinRating == b.MinRating && a.InStock == b.InStock && (a.Keywords ?? string.Empty).Equals(b.Keywords ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
} 