using System.Net.Mail;
using System.Text;
using Homework.Notifications.Configurations;

namespace Homework.Notifications.Services;

public class MessageFactory
{
    private readonly EmailSettings _settings;
    
    public MessageFactory(EmailSettings settings)
    {
        _settings = settings;
    }
    
    public MailMessage Create(string email)
    {
        var from = new MailAddress(_settings.SmtpReply, _settings.SmtpUser);
        var to = new MailAddress(email);
        
        var message = new MailMessage(from, to)
        {
            SubjectEncoding = Encoding.UTF8,
            Subject = "Notification!",
            BodyEncoding = Encoding.UTF8,
            Body = "Hello World!",
            IsBodyHtml = true,
        };
        
        return message;
    }
}