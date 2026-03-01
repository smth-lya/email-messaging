using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Homework.Api;

public class RedisCachedProductRepository : IProductRepository
{
    private readonly EfProductRepository _repository;
    private readonly IDatabase _cache;

    private readonly ProductCacheSettings _settings;
    
    public RedisCachedProductRepository(EfProductRepository repository, IConnectionMultiplexer redis, IOptions<ProductCacheSettings> options)
    {
        _repository = repository;
        _cache = redis.GetDatabase();
        _settings = options.Value;
    }
    
    public async Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"{_settings.KeyPrefix}:{id}";
        
        var cached = await _cache.StringGetAsync(key);
        if (cached.HasValue)
            return JsonSerializer.Deserialize<Product>(cached.ToString());
        
        var product = await _repository.GetAsync(id, cancellationToken);
        if (product is not null)
            await _cache.StringSetAsync(key, JsonSerializer.Serialize(product), TimeSpan.FromSeconds(_settings.TTL));
        
        return product;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(product, cancellationToken);
        
        // cache-aside подход
        await InvalidateAsync(product.Id);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(product, cancellationToken);
        await InvalidateAsync(product.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        await InvalidateAsync(id);
    }
    
    private async Task InvalidateAsync(Guid id)
    {
        var key = $"{_settings.KeyPrefix}:{id}";
        await _cache.KeyDeleteAsync(key);
    }
}