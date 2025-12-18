using System.Security.Claims;
using BusinessLogic.DTO;
using BusinessLogic.Services.Team;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taskify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController(ITeamService teamService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto team)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var parsedUserId = int.Parse(userId);
            
            var createdTeamId = await teamService.CreateTeam(team, parsedUserId);
            return Ok(new{teamId = createdTeamId, message = "Team created successfully!"});
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTeams()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var parsedUserId = int.Parse(userId);
            var teamsByUser = await teamService.GetUserTeams(parsedUserId);
            return Ok(teamsByUser);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("{teamId:int}/members")]
    public async Task<IActionResult> GetTeamMembers([FromRoute] int teamId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var parsedUserId = int.Parse(userId);
            var members = await teamService.GetTeamsMembers(teamId, parsedUserId);
            return Ok(members);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}