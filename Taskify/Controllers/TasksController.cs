using System.Security.Claims;
using BusinessLogic.DTO;
using BusinessLogic.Services.TaskService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taskify.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto task)
    {
        try 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var parsedUserId = int.Parse(userId);
            
            if (task.AssignedToUserId == null)
            {
                task.AssignedToUserId = parsedUserId;
            }
            
            await taskService.CreateTask(task, parsedUserId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message }); 
        }
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDto task)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var parsedUserId = int.Parse(userId);
            await taskService.UpdateTask(task, parsedUserId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpGet("team/{teamId}")]
    public async Task<IActionResult> GetTeamTasks([FromRoute] int teamId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var parsedUserId = int.Parse(userId);
            return Ok(await taskService.GetTeamTasks(teamId, parsedUserId));
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var parsedUserId = int.Parse(userId);
            return Ok(await taskService.GetMyTask(parsedUserId));
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpPost("comment")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto comment)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var parsedUserId = int.Parse(userId);
            await taskService.AddComment(comment, parsedUserId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}