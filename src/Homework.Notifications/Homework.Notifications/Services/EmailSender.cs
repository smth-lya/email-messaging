using System.Collections.Concurrent;
using Hangfire;
using Homework.Notifications.Models;
using Homework.Notifications.Services.Abstractions;

namespace Homework.Notifications.Services;

public class EmailSender : IEmailSender
{
    private readonly NetworkClient _client;
    private readonly IMessageFactory _factory;
    private readonly IBackgroundJobClient _hangfire;

    public EmailSender(IMessageFactory factory, NetworkClient client, IBackgroundJobClient hangfire)
    {
        _client = client;
        _factory = factory;
        _hangfire = hangfire;
    }
    
    public async Task SendEmailAsync(MessageData data)
    {
        var message = await _factory.CreateAsync(data);
        try
        {
            await _client.SendEmailAsync(message);
        }
        finally
        {
            message.Dispose();
        }
    }

    public async Task SendBulkEmailAsync(IEnumerable<MessageData> messages, CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();

        using var semaphore = new SemaphoreSlim(10);
        var tasks = new List<Task>();
        var failedMessages = new ConcurrentBag<(MessageData Data, Exception Error)>();

        foreach (var data in messageList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await semaphore.WaitAsync(cancellationToken);
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await SendEmailAsync(data);
                }
                catch (Exception e)
                {
                    failedMessages.Add((data, e));
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }
        
        await Task.WhenAll(tasks);

        if (!failedMessages.IsEmpty)
        {
            throw new AggregateException(
                $"Failed to send {failedMessages.Count} emails",
                failedMessages.Select(x => x.Error));
        }
    }

    public string SendEmailWithDelay(MessageData data, TimeSpan delay)
    {
        var jobId = _hangfire.Schedule<EmailSender>(sender => sender.SendEmailAsync(data), delay);
        return jobId;
    }
}