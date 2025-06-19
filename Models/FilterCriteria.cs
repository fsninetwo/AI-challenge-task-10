namespace AIConsoleApp.Models;

public class FilterCriteria
{
    public string? Category { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public bool? InStock { get; set; }
    public string? Keywords { get; set; }
} 