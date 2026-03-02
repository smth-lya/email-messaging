using Homework.Api.Models;

namespace Homework.Api.Database;

public class EfProductRepository : IProductRepository
{
    private readonly ProductCatalogDbContext _dbContext;

    public EfProductRepository(ProductCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // подумать, стоит ли менять FindAsync на SingleOrDefaultAsync or FirstOrDefaultAsync
        return await _dbContext.Products.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Products.FindAsync([product.Id], cancellationToken: cancellationToken);
        
        if (existing is not null)
        {
            return existing;
        }
        
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Products.FindAsync([product.Id], cancellationToken: cancellationToken);

        if (existing is null)
        {
            return;
        }

        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Products.FindAsync([id], cancellationToken: cancellationToken);

        if (existing is null)
        {
            return;
        }
        
        _dbContext.Products.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}