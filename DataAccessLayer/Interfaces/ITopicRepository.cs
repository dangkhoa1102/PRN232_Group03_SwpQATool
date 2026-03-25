using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces;

public interface ITopicRepository
{
    Task<IEnumerable<Topic>> GetAllAsync();
    Task<Topic?> GetByIdAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    Task<bool> IsUsedInGroupsAsync(Guid id);
    Task<Topic> CreateAsync(Topic topic);
    Task<Topic> UpdateAsync(Topic topic);
    Task DeleteAsync(Topic topic);
}
