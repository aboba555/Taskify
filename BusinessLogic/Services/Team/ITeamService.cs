using BusinessLogic.DTO;

namespace BusinessLogic.Services.Team;

public interface ITeamService
{
    Task<int> CreateTeam(CreateTeamDto createTeamDto, int userId);
    Task<List<TeamDto>> GetUserTeams(int userId);
    Task<List<TeamMemberDto>> GetTeamsMembers(int teamId, int requesterUserId);
}