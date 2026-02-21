using System.Net;
using System.Net.Mail;
using Homework.Notifications.Configurations;

namespace Homework.Notifications.Services;

public class NetworkClient
{
    private readonly SmtpClient _client;
    
    public NetworkClient(SmtpClient client)
    {
        _client = client;
    }

    public async Task SendEmailAsync(MailMessage mail)
    {
        await _client.SendMailAsync(mail);
    }
}