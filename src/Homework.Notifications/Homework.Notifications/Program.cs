using Homework.Notifications.Configurations;
using Homework.Notifications.Extensions;
using Homework.Notifications.Services.Abstractions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddEmailSender(builder.Configuration);

var app = builder.Build();

//app.MapGet("/users/notify/{email}", NotifyUser);
app.MapGet("/", (EmailServerSettings d) => $"Hello World! {d.EnableSsl} {d.Host}:{d.Port} {d.Password}");

app.Run();

return;

async Task<string> NotifyUser(string email, IEmailSender sender)
{
    await sender.SendEmailAsync(email);
    return "Email sent successfully!";
}