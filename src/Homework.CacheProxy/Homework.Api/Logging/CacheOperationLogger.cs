namespace Homework.Api.Logging;

public interface ICacheOperationLogger
{
    void LogCacheHit(string key, long elapsedMs);
    void LogCacheMiss(string key, long elapsedMs);
    void LogCacheInvalidation(string key, string operation);
    void LogCacheError(string key, Exception ex);
}

public class CacheOperationLogger : ICacheOperationLogger
{
    private readonly ILogger<CacheOperationLogger> _logger;

    public CacheOperationLogger(ILogger<CacheOperationLogger> logger)
    {
        _logger = logger;
    }

    public void LogCacheHit(string key, long elapsedMs)
    {
        _logger.LogInformation(
            "Cache hit: Key={CacheKey}, ElapsedMs={ElapsedMs}, Level=CACHE",
            key,
            elapsedMs
        );
    }

    public void LogCacheMiss(string key, long elapsedMs)
    {
        _logger.LogInformation(
            "Cache miss: Key={CacheKey}, ElapsedMs={ElapsedMs}, Level=DATABASE",
            key,
            elapsedMs
        );
    }

    public void LogCacheInvalidation(string key, string operation)
    {
        _logger.LogInformation(
            "Cache invalidated: Key={CacheKey}, Operation={Operation}",
            key,
            operation
        );
    }

    public void LogCacheError(string key, Exception ex)
    {
        _logger.LogError(
            ex,
            "Cache operation failed: Key={CacheKey}, ExceptionType={ExceptionType}",
            key,
            ex.GetType().Name
        );
    }
}