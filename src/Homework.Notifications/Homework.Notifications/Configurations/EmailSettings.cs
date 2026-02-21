namespace Homework.Notifications.Configurations;

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } 
    public string SmtpReply { get; set; }
    public string SmtpPassword { get; set; }
    public bool EnableSsl { get; set; }
    public string? DefaultFrom { get; set; }
    public string? DefaultFromName { get; set; }
}