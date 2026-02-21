namespace Homework.Notifications.Models;

public class SendBulkEmailRequest
{
    public List<SendEmailRequest> Messages { get; set; } = new();
}