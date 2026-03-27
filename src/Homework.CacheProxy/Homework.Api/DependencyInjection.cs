using Homework.Api.Configurations;
using Homework.Api.Database;
using Homework.Api.Logging;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Homework.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddCacheProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProductCacheSettings>(
            configuration.GetSection(nameof(ProductCacheSettings)));
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection") ?? string.Empty));

        services.AddDbContext<ProductCatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

        services.AddScoped<ICacheOperationLogger, CacheOperationLogger>();
        
        services.AddScoped<EfProductRepository>();
        services.AddScoped<IProductRepository, RedisCachedProductRepository>();
        
        return services;
    }
}