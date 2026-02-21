namespace Homework.Notifications.Configurations;

public class TemplateConfiguration
{
    public string Subject { get; set; }
    public string TemplateName { get; set; }
    public string DefaultTitle { get; set; }
    public string DefaultMessage { get; set; }
    public string? DefaultFooter { get; set; }
    public bool ShowButton { get; set; } = true;
    public string? ButtonDefaultText { get; set; }
    public string? ButtonDefaultUrl { get; set; }
}