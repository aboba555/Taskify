using BusinessLogic.DTO;
using BusinessLogic.Services.Auth;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Taskify.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : Controller
{
    [HttpPost("register")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto user)
    {
        try
        {
            await authService.Register(user);
            return Ok(new { message = "User is registered successfully" });
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto user)
    {
        try
        {
            var token = await authService.Login(user);
            return Ok(new { message = token });
        }
        catch (Exception e)
        {
            return Unauthorized(new {error = e.Message});
        }
    }

    [HttpPost("google-auth")]
    public async Task<IActionResult> GoogleAuth([FromBody]GoogleUserLoginDto googleUserLoginDto)
    {
        try
        {
            var token = await authService.GoogleLogin(googleUserLoginDto);
            return Ok(new { message = token });
        }
        catch (Exception e)
        {
            return Unauthorized(new {error = e.Message});
        }
    }
    
    [HttpPost("verify-email")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        try
        {
            await authService.VerifyEmail(dto);
            return Ok(new { message = "Email successfully verified. You can now login!" });
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpPost("resend-email")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<IActionResult> ResendEmail([FromBody] ResendEmailDto dto)
    {
        try
        {
            await authService.ResendVerificationEmail(dto);
            return Ok(new { message = "Verification email sent successfully" });
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new 
        { 
            message = "You are now logged in!", 
            email = email,
            id = userName 
        });
    }
}