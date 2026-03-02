using Homework.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Homework.Api.Database;

public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options) : base(options)
    { }
    
    public DbSet<Product> Products => Set<Product>();
}