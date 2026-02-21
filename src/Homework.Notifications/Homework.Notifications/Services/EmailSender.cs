using Homework.Notifications.Models;
using Homework.Notifications.Services.Abstractions;

namespace Homework.Notifications.Services;

public class EmailSender : IEmailSender
{
    private readonly NetworkClient _client;
    private readonly HtmlMessageFactory _factory;

    public EmailSender(HtmlMessageFactory factory, NetworkClient client)
    {
        _client = client;
        _factory = factory;
    }
    
    public async Task SendEmailAsync(MessageData data)
    {
        var message = await _factory.CreateAsync(data);
        try
        {
            await _client.SendEmailAsync(message);
        }
        finally
        {
            message.Dispose();
        }
    }
}