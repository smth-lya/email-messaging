namespace Homework.Api.Logging;

public static class StructuredLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseStructuredLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        
        return app.UseMiddleware<StructuredLoggingMiddleware>();
    }
}