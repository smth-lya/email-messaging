namespace Homework.Notifications.Models;

public class SendDelayedEmailRequest : SendEmailRequest
{
    public int DelaySeconds { get; set; } = 60;
    public int DelayMinutes { get; set; }
}