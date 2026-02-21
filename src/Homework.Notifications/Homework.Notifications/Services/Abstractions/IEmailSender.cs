using Homework.Notifications.Models;

namespace Homework.Notifications.Services.Abstractions;

public interface IEmailSender
{
    Task SendEmailAsync(MessageData data);
}