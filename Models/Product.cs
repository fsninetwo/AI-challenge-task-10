namespace AIConsoleApp.Models;
using System.Text.Json.Serialization;

public class Product
{
    [JsonPropertyName("name")] 
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")] 
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("price")] 
    public decimal Price { get; set; }

    [JsonPropertyName("rating")] 
    public double Rating { get; set; }

    [JsonPropertyName("in_stock")]
    public bool InStock { get; set; }
} 