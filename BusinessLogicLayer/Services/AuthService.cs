using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataAccessLayer.Interfaces;
using BusinessLogicLayer.DTOs.Auth;
using BusinessLogicLayer.Configuration;
using BusinessLogicLayer.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using BusinessLogicLayer.Interfaces;

namespace BusinessLogicLayer.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    public Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        _logger.LogInformation($"Login attempt for email: {request.Email}");
        
        var user = _userRepository.GetByEmail(request.Email);
        if (user is null)
        {
            _logger.LogWarning($"User not found with email: {request.Email}");
            return Task.FromResult<LoginResponseDto?>(null);
        }

        _logger.LogInformation($"User found: {user.Email}, Role: {user.Role}");
        
        var isValidPassword = PasswordHasherUtil.VerifyPassword(request.Password, user.Password);
        if (!isValidPassword)
        {
            _logger.LogWarning($"Invalid password for user: {request.Email}");
            return Task.FromResult<LoginResponseDto?>(null);
        }

        _logger.LogInformation($"Password verified for user: {request.Email}");
        
        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_jwtSettings.ExpiresMinutes);
        var token = GenerateToken(user.UserId, user.FullName, user.Email, user.Role, user.UserCode, expiresAtUtc);

        var result = new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            User = new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                UserCode = user.UserCode
            }
        };

        _logger.LogInformation($"Login successful for user: {request.Email}");
        return Task.FromResult<LoginResponseDto?>(result);
    }

    public UserDto? GetCurrentUser(ClaimsPrincipal principal)
    {
        var idString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var fullName = principal.FindFirst(ClaimTypes.Name)?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;
        var userCode = principal.FindFirst("UserCode")?.Value;

        if (string.IsNullOrWhiteSpace(idString) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(role) ||
            !Guid.TryParse(idString, out var id))
        {
            return null;
        }

        return new UserDto
        {
            UserId = id,
            FullName = fullName,
            Email = email,
            Role = role,
            UserCode = userCode ?? string.Empty
        };
    }

    private string GenerateToken(Guid userId, string fullName, string email, string role, string userCode, DateTime expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("UserCode", userCode),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
