using System.ComponentModel.DataAnnotations.Schema;

namespace Homework.Benchmarks.Models;

[Table("categories")]
public class Category
{
    [Column("categoryid")]
    public Guid CategoryId { get; set; }
    [Column("name")]
    public string Name { get; set; } = null!;
    [Column("description")]
    public string Description { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}