namespace Homework.Notifications.Services;

public class EmailSender
{
    private readonly NetworkClient _client;
    private readonly MessageFactory _factory;

    public EmailSender(MessageFactory factory, NetworkClient client)
    {
        _client = client;
        _factory = factory;
    }
    
    public void SendEmail(string to, string subject, string body)
    {
        var email = _factory.Create(to);
        _client.SendEmail(email);
        Console.WriteLine($"Email sent to {to}");
    }
}