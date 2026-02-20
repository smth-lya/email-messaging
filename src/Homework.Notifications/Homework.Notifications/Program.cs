using Homework.Notifications.Configurations;
using Homework.Notifications.Extensions;
using Homework.Notifications.Services.Abstractions;
using Microsoft.AspNetCore.Components.Web;

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

async Task<string> NotifyUser(string email, IEmailSender sender)
{
    await sender.SendEmailAsync(email);
    return "Email sent successfully!";
}