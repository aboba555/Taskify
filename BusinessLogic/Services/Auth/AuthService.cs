using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessLogic.DTO;
using BusinessLogic.Services.EmailService;
using DataAccess;
using DataAccess.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BusinessLogic.Services.Auth;

public class AuthService(AppDbContext appDbContext, IConfiguration configuration, IEmailService emailService) : IAuthService
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
            IsActivated = false
        };
        
        await appDbContext.Users.AddAsync(newUser);
        await appDbContext.SaveChangesAsync();

        var code = GenerateVerificationCode();

        var newVerificationCode = new EmailVerification
        {
            UserId = newUser.Id,
            Code = code,
            SentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        await appDbContext.EmailVerifications.AddAsync(newVerificationCode);
        await appDbContext.SaveChangesAsync();
        await emailService.SendVerificationEmail(newUser.Email, code);
    }

    public async Task<string> Login(LoginUserDto loginUserDto)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u=> u.Email == loginUserDto.Email);

        if (user == null)
        {
            throw new Exception("Invalid credentials");
        }

        if (user.Password == null)
        {
            throw new Exception("You should login differently");
        }
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.Password);

        if (!isValidPassword)
        {
            throw new Exception("Invalid credentials");
        }
        
        if (!user.IsActivated)
        {
            throw new Exception("Please verify your email first");
        }
        
        var token = GenerateJwtToken(user);
        return token;
    }

    public async Task<string> GoogleLogin(GoogleUserLoginDto googleUserLoginDto)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] {configuration["Google:ClientId"]}
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(googleUserLoginDto.GoogleToken, settings);
        if (payload == null)
        {
            throw new Exception("Invalid credentials");
        }

        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
        if (user != null)
        {
            if (user.GoogleId == payload.Subject)
            {
                return GenerateJwtToken(user);
            }
            throw new Exception("Email already exists");
        }

        var creatingUser = new User
        {
            FirstName = payload.GivenName,
            LastName = payload.FamilyName,
            Email = payload.Email,
            Password = null,
            GoogleId = payload.Subject,
            IsActivated = true
        };
        await appDbContext.Users.AddAsync(creatingUser);
        await appDbContext.SaveChangesAsync();
        return GenerateJwtToken(creatingUser);
    }

    public async Task VerifyEmail(VerifyEmailDto dto)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            throw new Exception("Invalid credentials");
        }
        
        var record =  await appDbContext.EmailVerifications.FirstOrDefaultAsync(v => v.UserId == user.Id && v.Code == dto.Code);
        if (record == null)
        {
            throw new Exception("Invalid credentials");
        }
        if (record.ExpiresAt < DateTime.UtcNow)
        {
            throw new Exception("Code expired");
        }
        user.IsActivated = true;
        appDbContext.EmailVerifications.Remove(record);
        await appDbContext.SaveChangesAsync();
    }

    public async Task ResendVerificationEmail(ResendEmailDto dto)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            throw new Exception("Invalid credentials");
        }
        
        var record =  await appDbContext.EmailVerifications.FirstOrDefaultAsync(v => v.UserId == user.Id);
        if (record == null)
        {
            throw new Exception("Invalid credentials");
        }
        var code = GenerateVerificationCode();
        
        record.Code = code;
        record.SentAt = DateTime.UtcNow;
        record.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
        
        await appDbContext.SaveChangesAsync();
        
        await emailService.SendVerificationEmail(user.Email, code);
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
    
    private string GenerateVerificationCode()
    {
        int code = RandomNumberGenerator.GetInt32(0, 1000000);
        return code.ToString("D6");
    }
}