using BusinessLogic.Services.Auth;
using BusinessLogic.Services.Invitation;
using BusinessLogic.Services.TaskService;
using BusinessLogic.Services.Team;
using BusinessLogic.Services.TelegramService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace BusinessLogic;

public static class Extension
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddScoped<IAuthService,AuthService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddSingleton<TelegramBotClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var token = config["Telegram:BotToken"];
            return new TelegramBotClient(token);
        });
        services.AddScoped<ITelegramService, TelegramService>();
        
        return services;
    }
    
}