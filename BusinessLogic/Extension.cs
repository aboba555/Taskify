using System.Net;
using System.Net.Mail;
using System.Threading.Channels;
using BusinessLogic.Services.Auth;
using BusinessLogic.Services.EmailService;
using BusinessLogic.Services.Invitation;
using BusinessLogic.Services.NotificationService;
using BusinessLogic.Services.NotificationWorker;
using BusinessLogic.Services.TaskService;
using BusinessLogic.Services.Team;
using BusinessLogic.Services.TelegramService;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace BusinessLogic;

public static class Extension
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthService,AuthService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<TelegramBotClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var token = config["Telegram:TokenKey"];
            if (string.IsNullOrEmpty(token))
                throw new Exception("Telegram:TokenKey is missing in configuration");
            return new TelegramBotClient(token);
        });
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddHostedService<NotificationWorker>();
        
        var senderEmail = configuration["EmailSettings:SenderEmail"];
        var senderName = configuration["EmailSettings:SenderName"];
        
        services
            .AddFluentEmail(senderEmail, senderName)
            .AddSmtpSender(new SmtpClient
            {
                Host = configuration["EmailSettings:Host"]!,
                Port = configuration.GetValue<int>("EmailSettings:Port"),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    configuration["EmailSettings:Username"], 
                    configuration["EmailSettings:Password"]
                )
            });
        
        return services;
    }
    
}