using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLogic.DTO;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BusinessLogic.Services.Auth;

public class AuthService(AppDbContext appDbContext, IConfiguration configuration) : IAuthService
{
    public async Task Register(RegisterUserDto registerUserDto)
    {
        var isEmailTaken = await appDbContext.Users.AnyAsync(x => x.Email == registerUserDto.Email);
        
        if (isEmailTaken)
        {
            throw new Exception("User with this email already exists");
        }
        
        var bCryptedPassword = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);
        
        var newUser = new User
        {
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            Email = registerUserDto.Email,
            Password = bCryptedPassword,
        };
        
        await appDbContext.Users.AddAsync(newUser);
        await appDbContext.SaveChangesAsync();
    }

    public async Task<string> Login(LoginUserDto loginUserDto)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u=> u.Email == loginUserDto.Email);

        if (user == null)
        {
            throw new Exception("Invalid credentials");
        }

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.Password);

        if (!isValidPassword)
        {
            throw new Exception("Invalid credentials");
        }
        
        var token = GenerateJwtToken(user);
        return token;
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = configuration["JwtSettings:SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("firstName", user.FirstName),
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["JwtSettings:ExpiresInMinutes"])),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}