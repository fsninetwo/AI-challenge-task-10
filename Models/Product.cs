namespace AIConsoleApp.Models;

public class Product
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public bool InStock { get; set; }
} 