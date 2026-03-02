using System.Text.Json;
using Homework.Api.Configurations;
using Homework.Api.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Homework.Api.Database;

public class RedisCachedProductRepository : IProductRepository
{
    private readonly EfProductRepository _repository;
    private readonly IDatabase _cache;

    private readonly ProductCacheSettings _settings;
    private readonly ILogger<RedisCachedProductRepository> _logger;
    
    public RedisCachedProductRepository(EfProductRepository repository, IConnectionMultiplexer redis, IOptions<ProductCacheSettings> options, ILogger<RedisCachedProductRepository> logger)
    {
        _repository = repository;
        _cache = redis.GetDatabase();
        _settings = options.Value;
        _logger = logger;
    }
    
    public async Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"{_settings.KeyPrefix}:{id}";
        
        var cached = await _cache.StringGetAsync(key);
        if (cached.HasValue)
        {
            _logger.LogInformation($"Product {id} was fetched from the CACHE");
            return JsonSerializer.Deserialize<Product>(cached.ToString());
        }
        
        var product = await _repository.GetAsync(id, cancellationToken);
        if (product is not null)
            await _cache.StringSetAsync(key, JsonSerializer.Serialize(product), TimeSpan.FromSeconds(_settings.TTL));
        
        _logger.LogInformation($"Product {id} was retrieved from the DATABASE");
        
        return product;
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        product = await _repository.AddAsync(product, cancellationToken);
        
        // cache-aside подход
        await InvalidateAsync(product.Id);

        return product;
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