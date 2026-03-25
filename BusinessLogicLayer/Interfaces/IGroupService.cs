namespace BusinessLogicLayer.Interfaces;

using BusinessLogicLayer.DTOs.Group;

public interface IGroupService
{
    Task<IEnumerable<GroupDto>> GetAllAsync();
    Task<GroupDetailDto?> GetByIdAsync(Guid id);
    Task<(GroupDto? Result, string? Error)> CreateAsync(GroupRequestDto request);
    Task<(GroupDto? Result, string? Error)> UpdateAsync(Guid id, GroupRequestDto request);
    Task<string?> DeleteAsync(Guid id);
    Task<string?> AddMemberAsync(Guid groupId, AddMemberDto request);
}
