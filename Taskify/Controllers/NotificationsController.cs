using System.Security.Claims;
using BusinessLogic.Services.NotificationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taskify.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var parsedUserId = int.Parse(userId);

            return Ok(await notificationService.GetAllNotificationsByUserId(parsedUserId));
        }catch (Exception ex)
        {
            return BadRequest(new {error = ex.Message});
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var parsedUserId = int.Parse(userId);

            return Ok(new {count = await notificationService.GetAllUnReadNotificationsByUserId(parsedUserId)});
        }
        catch (Exception e)
        {
            return BadRequest(new {error = e.Message});
        }
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkNotificationRead([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var parsedUserId = int.Parse(userId);

            await notificationService.MarkOneAsRead(id, parsedUserId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(new {error = e.Message});
        }
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var parsedUserId = int.Parse(userId);

            await notificationService.MarkAllAsRead(parsedUserId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}