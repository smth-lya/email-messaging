namespace Homework.Notifications.Configurations;

public record EmailSettings
{
    public string SmtpReply { get; set; }
    public string SmtpUser { get; set; }
}