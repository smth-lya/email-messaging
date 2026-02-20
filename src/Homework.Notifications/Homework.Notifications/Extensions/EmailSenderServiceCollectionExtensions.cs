using System.Net.Mail;
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
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<NetworkClient>();
            services.AddScoped<SmtpClient>();
            services.AddSingleton<MessageFactory>();

            services.AddSingleton<HtmlRenderer>(); // Подумать стоит ли изменить жизненный цикл на Scoped
            
            services.ConfigureSettings<EmailServerSettings>(configuration);
            services.ConfigureSettings<EmailSettings>(configuration);
        
            return services;
        }

        private void ConfigureSettings<T>(IConfiguration configuration) where T : class, new()
        {
            var settings = new T();
            configuration.GetSection(typeof(T).Name).Bind(settings);
            services.AddSingleton(settings);
        }
    }
}