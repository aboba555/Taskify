using BusinessLogic.DTO;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Team;

public class TeamService(AppDbContext context) : ITeamService
{
    public async Task<int> CreateTeam(CreateTeamDto createTeamDto, int userId)
    {
        var newTeam = new DataAccess.Models.Team
        {
            Name = createTeamDto.Name,
            Description = createTeamDto.Description,
            TeamUsers = new List<TeamUser>() 
        };
        
        var leaderLink = new TeamUser()
        {
            UserId = userId,
            Role = Role.Leader
        };
        
        newTeam.TeamUsers.Add(leaderLink);
        await context.Teams.AddAsync(newTeam);
        await context.SaveChangesAsync();
        
        return newTeam.Id;
    }

    public async Task<List<TeamDto>> GetUserTeams(int userId)
    {
        var teams = await context.TeamUsers
            .AsNoTracking()
            .Where(x=> x.UserId ==  userId)
            .Include(x=> x.Team)
            .Select(x=> new TeamDto
            {
                Id = x.Team.Id,
                Name = x.Team.Name,
                Description = x.Team.Description,
                Role = x.Role.ToString()
            }).ToListAsync();
         
        return teams;
    }
}