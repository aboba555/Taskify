using BusinessLogic.Services.Auth;
using BusinessLogic.Services.Team;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic;

public static class Extension
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddScoped<IAuthService,AuthService>();
        services.AddScoped<ITeamService, TeamService>();
        
        return services;
    }
    
}