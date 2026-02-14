using Homework.Notifications.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/user/notify", () => NotifyUser);
app.Run();

return;

string NotifyUser(string username, EmailSender sender)
{
    
}