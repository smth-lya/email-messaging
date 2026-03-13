namespace Homework.Benchmarks.Models;

public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    public int Stock { get; set; }
    public bool IsDeleted { get; set; }
}