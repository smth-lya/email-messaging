namespace Homework.Notifications.Services;

public class NetworkClient
{
    private readonly EmailServerSettings _settings;

    public NetworkClient(EmailServerSettings settings)
    {
        _settings = settings;
    }
}