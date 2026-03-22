using BusinessLogicLayer.DTOs.User;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Services;

public class UserService : IUserService
{
    private static readonly HashSet<string> ValidRoles =
        ["Admin", "Supervisor", "Reviewer", "Student"];

    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserListDto>> GetAllAsync(string? role, bool unassigned)
    {
        var users = await _userRepository.GetAllAsync(role, unassigned);
        return users.Select(MapToDto);
    }

    public async Task<UserListDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<(UserListDto? Result, string? Error, int StatusCode)> CreateAsync(CreateUserRequestDto request)
    {
        if (!ValidRoles.Contains(request.Role))
            return (null, $"Role không hợp lệ. Chỉ chấp nhận: {string.Join(", ", ValidRoles)}.", 400);

        if (await _userRepository.ExistsByUserCodeAsync(request.UserCode))
            return (null, $"Mã user '{request.UserCode}' đã tồn tại.", 409);

        if (await _userRepository.ExistsByEmailAsync(request.Email))
            return (null, $"Email '{request.Email}' đã tồn tại.", 409);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            UserCode = request.UserCode.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Password = request.Password,
            Role = request.Role,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
        };

        var created = await _userRepository.CreateAsync(user);
        return (MapToDto(created), null, 201);
    }

    public async Task<(UserListDto? Result, string? Error, int StatusCode)> UpdateAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return (null, "Không tìm thấy user.", 404);

        if (request.Role is not null && !ValidRoles.Contains(request.Role))
            return (null, $"Role không hợp lệ. Chỉ chấp nhận: {string.Join(", ", ValidRoles)}.", 400);

        if (request.Email is not null)
        {
            var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, excludeId: id);
            if (emailExists)
                return (null, $"Email '{request.Email}' đã tồn tại.", 409);
        }

        if (request.FullName is not null)
            user.FullName = request.FullName.Trim();

        if (request.Email is not null)
            user.Email = request.Email.Trim().ToLower();

        if (request.Role is not null)
            user.Role = request.Role;

        if (request.Password is not null)
            user.Password = request.Password;

        var updated = await _userRepository.UpdateAsync(user);
        return (MapToDto(updated), null, 200);
    }

    public async Task<(string? Error, int StatusCode)> DeleteAsync(Guid id, Guid currentUserId)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return ("Không tìm thấy user.", 404);

        if (user.UserId == currentUserId)
            return ("Admin không thể tự xóa chính mình.", 400);

        if (string.Equals(user.Role, "Supervisor", StringComparison.OrdinalIgnoreCase))
        {
            if (await _userRepository.IsSupervisorOfGroupAsync(id))
                return ("Supervisor đang hướng dẫn nhóm, không thể xóa.", 400);
        }

        if (string.Equals(user.Role, "Reviewer", StringComparison.OrdinalIgnoreCase))
        {
            if (await _userRepository.IsReviewerOfTopicAsync(id))
                return ("Reviewer đang phụ trách topic, không thể xóa.", 400);
        }

        await _userRepository.DeleteAsync(user);
        return (null, 204);
    }

    public async Task SeedDefaultAdminAsync()
    {
        if (await _userRepository.HasAnyUserAsync())
            return;

        var admin = new User
        {
            UserId = Guid.NewGuid(),
            UserCode = "ADM001",
            FullName = "System Admin",
            Email = "admin@swp.com",
            Password = "Admin@123",
            Role = "Admin",
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
        };

        await _userRepository.CreateAsync(admin);
    }

    private static UserListDto MapToDto(User u) => new()
    {
        UserId = u.UserId,
        UserCode = u.UserCode,
        FullName = u.FullName,
        Email = u.Email,
        Role = u.Role,
        CreatedAt = u.CreatedAt
    };
}
