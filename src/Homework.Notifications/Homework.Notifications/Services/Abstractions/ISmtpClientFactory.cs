using System.Net.Mail;

namespace Homework.Notifications.Services.Abstractions;

public interface ISmtpClientFactory
{
    SmtpClient CreateClient();
}