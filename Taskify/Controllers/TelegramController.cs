using System.Security.Claims;
using BusinessLogic.Services.TelegramService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace Taskify.Controllers;
[ApiController]
[Route("api/telegram")]
public class TelegramController(ITelegramService telegramService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Connect()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var parsedUserId = int.Parse(userId);
            
            var link = await telegramService.GenerateTelegramLink(parsedUserId);
            return Ok(new { link });
        }
        catch(Exception ex)
        {
            return BadRequest(new {error = ex.Message});
        }
    }
    
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update)
    {
        await telegramService.ProcessWebhook(update);
        return Ok();
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var parsedUserId = int.Parse(userId);
            var result = await telegramService.IsConnected(parsedUserId);
            return Ok(new { result });
        }
        catch (Exception e)
        {
            return BadRequest(new {error = e.Message});
        }
    }

    [Authorize]
    [HttpDelete("disconnect")]
    public async Task<IActionResult> Disconnect()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var parsedUserId = int.Parse(userId);
            await telegramService.Disconnect(parsedUserId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(new {error = e.Message});
        }
    }
    
}