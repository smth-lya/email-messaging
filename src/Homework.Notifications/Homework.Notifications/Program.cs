using Homework.Notifications.Extensions;
using Homework.Notifications.Models;
using Homework.Notifications.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddEmailSender(builder.Configuration);

var app = builder.Build();

app.MapGet("/users/notify/{email}", NotifyUser);

app.Run();

return;

async Task<string> NotifyUser(string email, IEmailSender sender, [FromQuery] string? mailTemplate)
{
    var messageData = new MessageData(email, mailTemplate ?? "Welcome");
    await sender.SendEmailAsync(messageData);
    return "Email sent successfully!";
}