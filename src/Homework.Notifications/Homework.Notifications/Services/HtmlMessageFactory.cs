using System.Net.Mail;
using System.Text;
using Homework.Notifications.Configurations;
using Homework.Notifications.Models;
using Homework.Notifications.Services.Abstractions;
using Homework.Notifications.templates;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Homework.Notifications.Services;

public class HtmlMessageFactory : IMessageFactory
{
    private readonly HtmlRenderer _htmlRenderer;
    private readonly EmailSettings _settings;
    private readonly NotificationTemplatesConfiguration _templateConfigs;
    
    public HtmlMessageFactory(
        HtmlRenderer htmlRenderer, 
        EmailSettings settings,
        NotificationTemplatesConfiguration templateConfigs)
    {
        _htmlRenderer = htmlRenderer;
        _settings = settings;
        _templateConfigs = templateConfigs;
    }
    
    public async Task<MailMessage> CreateAsync(MessageData data)
    {
        if (!_templateConfigs.Templates.TryGetValue(data.TemplateName, out var templateConfig))
        {
            throw new ArgumentException($"Template not found: {data.TemplateName}");
        }
        
        var from = new MailAddress(
            _settings.DefaultFrom ?? _settings.SmtpReply,
            _settings.DefaultFromName ?? _settings.SmtpUser);
        var to = new MailAddress(data.Email);

        var html = await RenderTemplateAsync(templateConfig);
        var subject = templateConfig.Subject;
        
        var message = new MailMessage(from, to)
        {
            SubjectEncoding = Encoding.UTF8,
            Subject = subject,
            BodyEncoding = Encoding.UTF8,
            Body = html,
            IsBodyHtml = true,
        };
        
        return message;
    }
    
    private async Task<string> RenderTemplateAsync(TemplateConfiguration templateConfig)
    {
        return await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = new Dictionary<string, object?>
            {
                ["Title"] = templateConfig.DefaultTitle,
                ["Message"] = templateConfig.DefaultMessage,
                ["FooterNote"] = templateConfig.DefaultFooter
            };

            if (templateConfig.ShowButton)
            {
                parameters["ButtonText"] = templateConfig.ButtonDefaultText;
                
                var buttonUrl = templateConfig.ButtonDefaultUrl ?? string.Empty;
                parameters["ButtonUrl"] = buttonUrl;
            }
            
            var parameterView = ParameterView.FromDictionary(parameters);
            var output = await _htmlRenderer.RenderComponentAsync<NotificationMessage>(parameterView);
            
            return output.ToHtmlString();
        });
    }
}