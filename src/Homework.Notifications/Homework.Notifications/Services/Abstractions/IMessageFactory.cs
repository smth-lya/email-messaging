using System.Net.Mail;
using Homework.Notifications.Models;

namespace Homework.Notifications.Services.Abstractions;

public interface IMessageFactory
{
    Task<MailMessage> CreateAsync(MessageData data);
}