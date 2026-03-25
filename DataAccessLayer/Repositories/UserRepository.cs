using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class UserRepository : IUserRepository
{
    private readonly swp_qa_toolsContext _context;

    public UserRepository(swp_qa_toolsContext context)
    {
        _context = context;
    }

    // --- Existing (sync) ---

    public User? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
    }

    public User? GetById(Guid id)
    {
        return _context.Users.FirstOrDefault(u => u.UserId == id);
    }

    // --- New (async) ---

    public async Task<IEnumerable<User>> GetAllAsync(string? role, bool unassigned)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (unassigned)
            query = query.Where(u => u.Role == "Student" && !u.GroupMembers.Any());

        return await query.OrderBy(u => u.FullName).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower()
                           && (excludeId == null || u.UserId != excludeId));
    }

    public async Task<bool> ExistsByUserCodeAsync(string userCode, Guid? excludeId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.UserCode.ToLower() == userCode.ToLower()
                           && (excludeId == null || u.UserId != excludeId));
    }

    public async Task<bool> IsSupervisorOfGroupAsync(Guid userId)
    {
        return await _context.Groups.AnyAsync(g => g.SupervisorId == userId);
    }

    public async Task<bool> IsReviewerOfTopicAsync(Guid userId)
    {
        return await _context.Topics.AnyAsync(t => t.ReviewerId == userId);
    }

    public async Task<bool> IsStudentInGroupAsync(Guid userId)
    {
    return await _context.GroupMembers.AnyAsync(gm => gm.StudentId == userId);
    }

    public async Task<bool> HasAnyUserAsync()
    {
        return await _context.Users.AnyAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(User user)
    {
        // For Students: remove from group_members first (in transaction)
        if (string.Equals(user.Role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa tất cả group_members liên quan đến student này
                var groupMembers = _context.GroupMembers.Where(gm => gm.StudentId == user.UserId);
                _context.GroupMembers.RemoveRange(groupMembers);
                await _context.SaveChangesAsync();

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        else
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
