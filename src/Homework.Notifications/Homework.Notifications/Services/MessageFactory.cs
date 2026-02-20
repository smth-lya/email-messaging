using System.Net.Mail;
using System.Text;
using Homework.Notifications.Configurations;
using Homework.Notifications.templates;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Homework.Notifications.Services;

public class MessageFactory
{
    private readonly HtmlRenderer _htmlRenderer;
    private readonly EmailSettings _settings;
    
    public MessageFactory(HtmlRenderer htmlRenderer, EmailSettings settings)
    {
        _htmlRenderer = htmlRenderer;
        _settings = settings;
    }
    
    public async Task<MailMessage> CreateAsync(string email)
    {
        var from = new MailAddress(_settings.SmtpReply, _settings.SmtpUser);
        var to = new MailAddress(email);

        var html = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>()
            {
                { "Message", "Hello Message"}
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await _htmlRenderer.RenderComponentAsync<NotificationMessage>(parameters);
            
            return output.ToHtmlString();
        });
        
        var message = new MailMessage(from, to)
        {
            SubjectEncoding = Encoding.UTF8,
            Subject = "Notification!",
            BodyEncoding = Encoding.UTF8,
            Body = html,
            IsBodyHtml = true,
        };
        
        return message;
    }
}