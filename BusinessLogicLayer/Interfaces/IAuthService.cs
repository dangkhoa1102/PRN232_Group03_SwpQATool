namespace BusinessLogicLayer.Interfaces;

using BusinessLogicLayer.DTOs.Auth;
using System.Security.Claims;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    UserDto? GetCurrentUser(ClaimsPrincipal principal);
}
