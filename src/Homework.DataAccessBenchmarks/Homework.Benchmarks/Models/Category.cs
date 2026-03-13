namespace Homework.Benchmarks.Models;

public class Category
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}