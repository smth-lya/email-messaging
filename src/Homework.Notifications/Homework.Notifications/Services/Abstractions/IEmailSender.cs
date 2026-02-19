namespace Homework.Notifications.Services.Abstractions;

public interface IEmailSender
{
    Task SendEmailAsync(string email);
}