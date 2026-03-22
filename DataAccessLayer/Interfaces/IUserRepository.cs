using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces;

public interface IUserRepository
{
    // Existing
    User? GetByEmail(string email);
    User? GetById(Guid id);

    // New
    Task<IEnumerable<User>> GetAllAsync(string? role, bool unassigned);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    Task<bool> ExistsByUserCodeAsync(string userCode, Guid? excludeId = null);
    Task<bool> IsSupervisorOfGroupAsync(Guid userId);
    Task<bool> IsReviewerOfTopicAsync(Guid userId);
    Task<bool> IsStudentInGroupAsync(Guid userId);
    Task<bool> HasAnyUserAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(User user);
}
