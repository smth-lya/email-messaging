using Homework.Api.Models;

namespace Homework.Api.Database;

public interface IProductRepository
{
    Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}