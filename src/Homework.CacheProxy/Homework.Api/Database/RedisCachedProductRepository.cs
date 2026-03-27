using System.Diagnostics;
using System.Text.Json;
using Homework.Api.Configurations;
using Homework.Api.Logging;
using Homework.Api.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Homework.Api.Database;

public class RedisCachedProductRepository : IProductRepository
{
    private readonly EfProductRepository _repository;
    private readonly IDatabase _cache;

    private readonly ProductCacheSettings _settings;
    private readonly ICacheOperationLogger _cacheLogger;
    private readonly ILogger<RedisCachedProductRepository> _logger;
    
    public RedisCachedProductRepository(
        EfProductRepository repository, 
        IConnectionMultiplexer redis, 
        IOptions<ProductCacheSettings> options, 
        ICacheOperationLogger cacheLogger,
        ILogger<RedisCachedProductRepository> logger)
    {
        _repository = repository;
        _cache = redis.GetDatabase();
        _settings = options.Value;
        _cacheLogger = cacheLogger;
        _logger = logger;
    }
    
    public async Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"{_settings.KeyPrefix}:{id}";
        var sw = Stopwatch.StartNew();

        try
        {
            var cached = await _cache.StringGetAsync(key);
            sw.Stop();
            
            if (cached.HasValue)
            {
                _cacheLogger.LogCacheHit(key, sw.ElapsedMilliseconds);
                return JsonSerializer.Deserialize<Product>(cached.ToString());
            }
        
            sw.Restart();  
            var product = await _repository.GetAsync(id, cancellationToken);
            sw.Stop();
            
            if (product is not null)
                await _cache.StringSetAsync(key, JsonSerializer.Serialize(product), TimeSpan.FromSeconds(_settings.TTL));
        
            _cacheLogger.LogCacheMiss(key, sw.ElapsedMilliseconds);
            
            return product;
        }
        catch (RedisConnectionException ex)
        {
            sw.Stop();
            
            _cacheLogger.LogCacheError(key, ex);
            return await _repository.GetAsync(id, cancellationToken);
        }
        
        
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        product = await _repository.AddAsync(product, cancellationToken);
        
        // cache-aside подход
        await InvalidateAsync(product.Id);

        _cacheLogger.LogCacheInvalidation($"{_settings.KeyPrefix}:{product.Id}", "ADD");
        
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(product, cancellationToken);
        await InvalidateAsync(product.Id);
        
        _cacheLogger.LogCacheInvalidation($"{_settings.KeyPrefix}:{product.Id}", "UPDATE");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        await InvalidateAsync(id);
        
        _cacheLogger.LogCacheInvalidation($"{_settings.KeyPrefix}:{id}", "DELETE");
    }
    
    private async Task InvalidateAsync(Guid id)
    {
        var key = $"{_settings.KeyPrefix}:{id}";
        await _cache.KeyDeleteAsync(key);
    }
}