using System.Security.Claims;
using BusinessLogicLayer.DTOs.Auth;

namespace BusinessLogicLayer.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    UserDto? GetCurrentUser(ClaimsPrincipal principal);
}
