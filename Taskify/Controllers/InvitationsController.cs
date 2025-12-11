using System.Security.Claims;
using BusinessLogic.DTO;
using BusinessLogic.Services.Invitation;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taskify.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationsController(IInvitationService invitationService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendInviteDto invitation)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            int parsedUserId = int.Parse(userId);
            
            await invitationService.SendInvite(invitation, parsedUserId);
            return Ok();
            
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyInvitations()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            int parsedUserId = int.Parse(userId);
            
            var invites = await invitationService.GetAllInvites(parsedUserId);
            return Ok(invites);
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptInvitation([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            int parsedUserId = int.Parse(userId);
            
            await invitationService.AcceptInvite(id, parsedUserId);
            
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }
    
    
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectInvitation([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            int parsedUserId = int.Parse(userId);
            
            await invitationService.DeclineInvite(id, parsedUserId);
            
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }
}