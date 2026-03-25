using BusinessLogicLayer.DTOs.User;

namespace BusinessLogicLayer.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserListDto>> GetAllAsync(string? role, bool unassigned);
    Task<UserListDto?> GetByIdAsync(Guid id);
    Task<(UserListDto? Result, string? Error, int StatusCode)> CreateAsync(CreateUserRequestDto request);
    Task<(UserListDto? Result, string? Error, int StatusCode)> UpdateAsync(Guid id, UpdateUserRequestDto request);
    Task<(string? Error, int StatusCode)> DeleteAsync(Guid id, Guid currentUserId);
    Task SeedDefaultAdminAsync();
}
