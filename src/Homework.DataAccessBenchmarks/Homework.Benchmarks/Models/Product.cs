using System.ComponentModel.DataAnnotations.Schema;

namespace Homework.Benchmarks.Models;

[Table("products")]
public class Product
{
    [Column("productid")]
    public Guid ProductId { get; set; }
    [Column("name")]
    public string Name { get; set; } = null!;
    [Column("description")]
    public string Description { get; set; } = null!;
    [Column("price")]
    public decimal Price { get; set; }
    [Column("categoryid")]
    public Guid CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    [Column("stock")]
    public int Stock { get; set; }
    [Column("isdeleted")]
    public bool IsDeleted { get; set; }
}