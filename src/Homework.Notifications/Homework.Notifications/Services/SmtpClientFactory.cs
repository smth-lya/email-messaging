using System.Net;
using System.Net.Mail;
using Homework.Notifications.Configurations;
using Homework.Notifications.Services.Abstractions;

namespace Homework.Notifications.Services;

public class SmtpClientFactory : ISmtpClientFactory
{
    private readonly EmailSettings _settings;
    
    public SmtpClientFactory(EmailSettings settings)
    {
        _settings = settings;
    }
    
    public SmtpClient CreateClient()
    {
        return new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
        {
            UseDefaultCredentials = false,
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
    }
}