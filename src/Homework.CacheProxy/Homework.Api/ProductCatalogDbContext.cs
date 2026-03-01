using Microsoft.EntityFrameworkCore;

namespace Homework.Api;

public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options) : base(options)
    { }
    
    public DbSet<Product> Products => Set<Product>();
}