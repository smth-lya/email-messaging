using System.Net.Mail;
using Hangfire;
using Hangfire.MemoryStorage;
using Homework.Notifications.Configurations;
using Homework.Notifications.Services;
using Homework.Notifications.Services.Abstractions;
using Microsoft.AspNetCore.Components.Web;

namespace Homework.Notifications.Extensions;

public static class EmailSenderServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEmailSender(IConfiguration configuration)
        {
            services.AddScoped<NetworkClient>();
            services.AddScoped<SmtpClient>(provider => provider.GetRequiredService<ISmtpClientFactory>().CreateClient());
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IMessageFactory, HtmlMessageFactory>();
            
            services.AddSingleton<HtmlRenderer>(); 
            services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();

            services.AddHangfire(config
                => config.UseMemoryStorage());
            services.AddHangfireServer();
            
            services.ConfigureSettings<EmailSettings>(configuration);
            services.ConfigureSettings<NotificationTemplatesConfiguration>(configuration, "NotificationTemplates");

            return services;
        }

        private void ConfigureSettings<T>(IConfiguration configuration, string? name = null) where T : class, new()
        {
            var settings = new T();
            configuration.GetSection(name ?? typeof(T).Name).Bind(settings);
            services.AddSingleton(settings);
        }
    }
}