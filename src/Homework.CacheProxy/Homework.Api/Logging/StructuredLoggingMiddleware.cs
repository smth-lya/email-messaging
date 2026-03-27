using System.Diagnostics;
using Serilog.Context;

namespace Homework.Api.Logging;

public class StructuredLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StructuredLoggingMiddleware> _logger;

    public StructuredLoggingMiddleware(
        RequestDelegate next,
        ILogger<StructuredLoggingMiddleware> logger)
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
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            var sw = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                sw.Stop();

                LogResponse(context, sw.ElapsedMilliseconds, responseBody);

                responseBody.Seek(0, SeekOrigin.Begin);
                context.Response.ContentLength = responseBody.Length;

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                sw.Stop();

                LogException(context, sw.ElapsedMilliseconds, ex);

                context.Response.Body = originalBodyStream;

                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    private void LogResponse(
        HttpContext context, 
        long elapsedMs,
        MemoryStream responseBody)
    {
        var logLevel =
            context.Response.StatusCode >= 500 ? LogLevel.Error :
            context.Response.StatusCode >= 400 ? LogLevel.Warning :
            LogLevel.Information;

        _logger.Log(
            logLevel,
            "Request completed: Method={Method}, Path={Path}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs}, ContentLength={ContentLength}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs,
            responseBody.Length
        );
    }
    
    private void LogException(
        HttpContext context, 
        long elapsedMs, 
        Exception ex)
    {
        _logger.LogError(
            ex,
            "Request failed: Method={Method}, Path={Path}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs}, ExceptionType={ExceptionType}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs,
            ex.GetType().Name
        );
    }
}