using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly swp_qa_toolsContext _context;

    public GroupRepository(swp_qa_toolsContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Group>> GetAllAsync()
    {
        return await _context.Groups
            .Include(g => g.Topic)
            .Include(g => g.Supervisor)
            .Include(g => g.Students)
            .OrderBy(g => g.GroupName)
            .ToListAsync();
    }

    public async Task<Group?> GetByIdAsync(Guid id)
    {
        return await _context.Groups
            .Include(g => g.Topic)
            .Include(g => g.Supervisor)
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.GroupId == id);
    }

    public async Task<bool> ExistsByNameInTopicAsync(string name, Guid topicId, Guid? excludeId = null)
    {
        return await _context.Groups
            .AnyAsync(g => g.GroupName.ToLower() == name.ToLower()
                           && g.TopicId == topicId
                           && (excludeId == null || g.GroupId != excludeId));
    }

    public async Task<bool> HasQuestionsAsync(Guid id)
    {
        return await _context.Questions.AnyAsync(q => q.GroupId == id);
    }

    public async Task<Group> CreateAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<Group> UpdateAsync(Group group)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task DeleteAsync(Group group)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove all group_members rows first
            group.Students.Clear();
            await _context.SaveChangesAsync();

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> IsStudentInGroupAsync(Guid groupId, Guid studentId)
    {
        return await _context.Groups
            .AnyAsync(g => g.GroupId == groupId && g.Students.Any(s => s.UserId == studentId));
    }

    public async Task<bool> IsStudentInAnyGroupAsync(Guid studentId)
    {
        return await _context.Groups
            .AnyAsync(g => g.Students.Any(s => s.UserId == studentId));
    }

    public async Task AddMemberAsync(Guid groupId, Guid studentId)
    {
        var group = await _context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        var student = await _context.Users.FindAsync(studentId);

        group!.Students.Add(student!);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid groupId, Guid studentId)
    {
        var group = await _context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        var student = group!.Students.FirstOrDefault(s => s.UserId == studentId);
        if (student is not null)
        {
            group.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}
