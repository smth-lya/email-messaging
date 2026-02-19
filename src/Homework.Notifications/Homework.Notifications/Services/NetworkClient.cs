using System.Net;
using System.Net.Mail;
using Homework.Notifications.Configurations;

namespace Homework.Notifications.Services;

public class NetworkClient
{
    private readonly EmailServerSettings _settings;
    private readonly SmtpClient _client;
    
    public NetworkClient(EmailServerSettings settings, SmtpClient client)
    {
        _settings = settings;
        _client = client;
        
        _client.Host = _settings.Host;
        _client.Port = _settings.Port;
        _client.UseDefaultCredentials = false;
        _client.EnableSsl = _settings.EnableSsl;
        _client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
        _client.DeliveryMethod = SmtpDeliveryMethod.Network;
    }

    public async Task SendEmailAsync(MailMessage mail)
    {
        await _client.SendMailAsync(mail);
    }
}