using Homework.Notifications.Models;
using Homework.Notifications.Services.Abstractions;

namespace Homework.Notifications.Extensions;

public static class EmailEndpoints
{
    public static IEndpointRouteBuilder MapEmailEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/email");

        group.MapPost("/send", SendEmailAsync);
        group.MapPost("/send-bulk", SendBulkEmailAsync);
        group.MapPost("/send-delayed", SendDelayedEmailAsync);

        return endpoints;
    }
    
    private static async Task<IResult> SendEmailAsync(SendEmailRequest request, IEmailSender sender)
    {
        try
        {
            var messageData = new MessageData(request.Email, request.TemplateName);

            await sender.SendEmailAsync(messageData);

            return Results.Ok(new
            {
                Message = $"Email sent to {request.Email}",
                SentAt = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                Error = ex.Message,
                ErrorType = "ValidationError"
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new
            {
                Error = "Failed to send email",
                ErrorType = "SendError",
                Details = ex.Message
            });
        }
    }

    private static async Task<IResult> SendBulkEmailAsync(SendBulkEmailRequest request, IEmailSender sender)
    {
        try
        {
            if (request.Messages.Count == 0)
            {
                return Results.BadRequest(new
                {
                    Error = "No messages provided",
                    ErrorType = "ValidationError"
                });
            }

            var messageDatas = request.Messages.Select(m => new MessageData(m.Email, m.TemplateName));

            await sender.SendBulkEmailAsync(messageDatas);

            return Results.Ok(new
            {
                Message = $"Bulk email sent to {request.Messages.Count} recipients",
                Recipients = request.Messages.Select(m => m.Email).ToList(),
                SentAt = DateTime.UtcNow
            });
        }
        catch (AggregateException ex)
        {
            return Results.BadRequest(new
            {
                Error = "Some emails failed to send",
                ErrorType = "PartialFailure",
                Failures = ex.InnerExceptions.Select(e => e.Message).ToList()
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new
            {
                Error = "Failed to send bulk emails",
                ErrorType = "SendError",
                Details = ex.Message
            });
        }
    }

    private static IResult SendDelayedEmailAsync(SendDelayedEmailRequest request, IEmailSender sender, HttpContext context)
    {
        try
        {
            var messageData = new MessageData(request.Email, request.TemplateName);

            var delay = request.DelayMinutes > 0 
                ? TimeSpan.FromMinutes(request.DelayMinutes)
                : TimeSpan.FromSeconds(request.DelaySeconds);
            
            var jobId = sender.SendEmailWithDelay(messageData, delay);
            
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var jobDetailsUrl = $"/hangfire/jobs/details/{jobId}";
            
            var response = new
            {
                Message = "Email scheduled with Hangfire",
                ScheduledFor = DateTime.UtcNow.Add(delay),
                JobId = jobId,
                AbsoluteHref = $"{baseUrl}{jobDetailsUrl}"
            };
            
            return Results.Accepted(jobDetailsUrl, response);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new
            {
                Error = "Failed to schedule email",
                ErrorType = "ScheduleError",
                Details = ex.Message
            });
        }
    }
}