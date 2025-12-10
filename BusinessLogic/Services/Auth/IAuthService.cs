using BusinessLogic.DTO;

namespace BusinessLogic.Services.Auth;

public interface IAuthService
{
    public Task Register(RegisterUserDto registerUserDto);
    Task<string> Login(LoginUserDto dto);
}