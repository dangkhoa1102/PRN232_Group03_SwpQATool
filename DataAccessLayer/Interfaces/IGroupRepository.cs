using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces;

public interface IGroupRepository
{
    Task<IEnumerable<Group>> GetAllAsync();
    Task<Group?> GetByIdAsync(Guid id);
    Task<IEnumerable<Group>> GetBySupervisorIdAsync(Guid supervisorId);
    Task<bool> ExistsByIdAndSupervisorIdAsync(Guid groupId, Guid supervisorId);
    Task<bool> ExistsByNameInTopicAsync(string name, Guid topicId, Guid? excludeId = null);
    Task<bool> HasQuestionsAsync(Guid id);
    Task<Group> CreateAsync(Group group);
    Task<Group> UpdateAsync(Group group);
    Task DeleteAsync(Group group);

    Task<bool> IsStudentInGroupAsync(Guid groupId, Guid studentId);
    Task<bool> IsStudentInAnyGroupAsync(Guid studentId);
    Task AddMemberAsync(Guid groupId, Guid studentId);
    Task RemoveMemberAsync(Guid groupId, Guid studentId);
}
