using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly swp_qa_toolsContext _context;

    public TopicRepository(swp_qa_toolsContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Topic>> GetAllAsync()
    {
        return await _context.Topics
            .Include(t => t.Reviewer)
            .OrderBy(t => t.TopicName)
            .ToListAsync();
    }

    public async Task<Topic?> GetByIdAsync(Guid id)
    {
        return await _context.Topics
            .Include(t => t.Reviewer)
            .FirstOrDefaultAsync(t => t.TopicId == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        return await _context.Topics
            .AnyAsync(t => t.TopicName.ToLower() == name.ToLower()
                           && (excludeId == null || t.TopicId != excludeId));
    }

    public async Task<bool> IsUsedInGroupsAsync(Guid id)
    {
        return await _context.Groups.AnyAsync(g => g.TopicId == id);
    }

    public async Task<Topic> CreateAsync(Topic topic)
    {
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    public async Task<Topic> UpdateAsync(Topic topic)
    {
        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    public async Task DeleteAsync(Topic topic)
    {
        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();
    }
}
