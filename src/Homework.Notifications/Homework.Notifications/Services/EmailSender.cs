using Homework.Notifications.Services.Abstractions;

namespace Homework.Notifications.Services;

public class EmailSender : IEmailSender
{
    private readonly NetworkClient _client;
    private readonly MessageFactory _factory;

    public EmailSender(MessageFactory factory, NetworkClient client)
    {
        _client = client;
        _factory = factory;
    }
    
    public async Task SendEmailAsync(string email)
    {
        var message = _factory.Create(email);
        await _client.SendEmailAsync(message);
        Console.WriteLine($"Email sent to {email}");
    }
}