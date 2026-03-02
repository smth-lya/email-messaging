namespace Homework.Api.Configurations;

public class ProductCacheSettings
{
    public int TTL { get; set; }
    public string KeyPrefix { get; set; }
}