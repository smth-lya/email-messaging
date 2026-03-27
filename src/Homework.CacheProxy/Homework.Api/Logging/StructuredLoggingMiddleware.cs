using System.Diagnostics;

namespace Homework.Api.Logging;

public class StructuredLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StructuredLoggingMiddleware> _logger;

    public StructuredLoggingMiddleware(RequestDelegate next, ILogger<StructuredLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationIdHeader)
            ? correlationIdHeader.ToString()
            : Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Append("X-Correlation-ID", correlationId);

        var sw = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            sw.Stop();

            await LogResponseAsync(context, correlationId, sw.ElapsedMilliseconds, responseBody);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogException(context, correlationId, sw.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogResponseAsync(
        HttpContext context, 
        string correlationId, 
        long elapsedMs,
        MemoryStream responseBody)
    {
        var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error
                : context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        
        _logger.Log(
            logLevel,
            "Request completed: CorrelationId={CorrelationId}, Method={Method}, Path={Path}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs}, ContentLength={ContentLength}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs,
            responseBody.Length
        );
    }
    
    private void LogException(
        HttpContext context, 
        string correlationId, 
        long elapsedMs, 
        Exception ex)
    {
        _logger.LogError(
            ex,
            "Request failed: CorrelationId={CorrelationId}, Method={Method}, Path={Path}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs}, ExceptionType={ExceptionType}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs,
            ex.GetType().Name
        );
    }
}