using Homework.Notifications.Models;

namespace Homework.Notifications.Services.Abstractions;

public interface IEmailSender
{
    Task SendEmailAsync(MessageData data);
    Task SendBulkEmailAsync(IEnumerable<MessageData> messages, CancellationToken cancellationToken = default);
    string SendEmailWithDelay(MessageData data, TimeSpan delay);
}