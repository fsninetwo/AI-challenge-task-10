using System.Text.Json.Serialization;

namespace AIConsoleApp.Models;

public class FilterCriteria
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("max_price")]
    public decimal? MaxPrice { get; set; }

    [JsonPropertyName("min_rating")]
    public double? MinRating { get; set; }

    [JsonPropertyName("in_stock")]
    public bool? InStock { get; set; }

    [JsonPropertyName("keywords")]
    public string? Keywords { get; set; }
} 