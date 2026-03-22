using BusinessLogicLayer.DTOs.Group;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IUserRepository _userRepository;

    public GroupService(IGroupRepository groupRepository, ITopicRepository topicRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _topicRepository = topicRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<GroupDto>> GetAllAsync()
    {
        var groups = await _groupRepository.GetAllAsync();
        return groups.Select(MapToDto);
    }

    public async Task<GroupDetailDto?> GetByIdAsync(Guid id)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        return group is null ? null : MapToDetailDto(group);
    }

    public async Task<(GroupDto? Result, string? Error)> CreateAsync(GroupRequestDto request)
    {
        var error = await ValidateAsync(request);
        if (error is not null)
            return (null, error);

        if (await _groupRepository.ExistsByNameInTopicAsync(request.GroupName, request.TopicId))
            return (null, $"A group named '{request.GroupName}' already exists in this topic.");

        var group = new Group
        {
            GroupId = Guid.NewGuid(),
            GroupName = request.GroupName.Trim(),
            TopicId = request.TopicId,
            SupervisorId = request.SupervisorId,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
        };

        var created = await _groupRepository.CreateAsync(group);
        var withNav = await _groupRepository.GetByIdAsync(created.GroupId);
        return (MapToDto(withNav!), null);
    }

    public async Task<(GroupDto? Result, string? Error)> UpdateAsync(Guid id, GroupRequestDto request)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group is null)
            return (null, "Group not found.");

        var error = await ValidateAsync(request);
        if (error is not null)
            return (null, error);

        if (await _groupRepository.ExistsByNameInTopicAsync(request.GroupName, request.TopicId, excludeId: id))
            return (null, $"A group named '{request.GroupName}' already exists in this topic.");

        group.GroupName = request.GroupName.Trim();
        group.TopicId = request.TopicId;
        group.SupervisorId = request.SupervisorId;

        var updated = await _groupRepository.UpdateAsync(group);
        var withNav = await _groupRepository.GetByIdAsync(updated.GroupId);
        return (MapToDto(withNav!), null);
    }

    public async Task<string?> DeleteAsync(Guid id)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group is null)
            return "Group not found.";

        if (await _groupRepository.HasQuestionsAsync(id))
            return "Cannot delete group because it has questions associated with it.";

        await _groupRepository.DeleteAsync(group);
        return null;
    }

    public async Task<string?> AddMemberAsync(Guid groupId, AddMemberDto request)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group is null)
            return "Group not found.";

        var student = _userRepository.GetById(request.StudentId);
        if (student is null)
            return "student_id does not correspond to any existing user.";

        if (!string.Equals(student.Role, "Student", StringComparison.OrdinalIgnoreCase))
            return $"User '{student.FullName}' does not have the Student role.";

        if (await _groupRepository.IsStudentInGroupAsync(groupId, request.StudentId))
            return $"Student '{student.FullName}' is already a member of this group.";

        if (await _groupRepository.IsStudentInAnyGroupAsync(request.StudentId))
            return $"Student '{student.FullName}' already belongs to another group.";

        await _groupRepository.AddMemberAsync(groupId, request.StudentId);
        return null;
    }

    public async Task<string?> RemoveMemberAsync(Guid groupId, Guid studentId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group is null)
            return "Group not found.";

        if (!await _groupRepository.IsStudentInGroupAsync(groupId, studentId))
            return "Student is not a member of this group.";

        await _groupRepository.RemoveMemberAsync(groupId, studentId);
        return null;
    }

    // BR-10: supervisor_id must have role = 'Supervisor'; topic_id must exist
    private async Task<string?> ValidateAsync(GroupRequestDto request)
    {
        var topic = await _topicRepository.GetByIdAsync(request.TopicId);
        if (topic is null)
            return "topic_id does not correspond to any existing topic.";

        var supervisor = _userRepository.GetById(request.SupervisorId);
        if (supervisor is null)
            return "supervisor_id does not correspond to any existing user.";

        if (!string.Equals(supervisor.Role, "Supervisor", StringComparison.OrdinalIgnoreCase))
            return $"User '{supervisor.FullName}' does not have the Supervisor role.";

        return null;
    }

    private static GroupDto MapToDto(Group g) => new()
    {
        GroupId = g.GroupId,
        GroupName = g.GroupName,
        TopicId = g.TopicId,
        TopicName = g.Topic?.TopicName ?? string.Empty,
        SupervisorId = g.SupervisorId,
        SupervisorName = g.Supervisor?.FullName ?? string.Empty,
        MemberCount = g.Students.Count,
        CreatedAt = g.CreatedAt
    };

    private static GroupDetailDto MapToDetailDto(Group g) => new()
    {
        GroupId = g.GroupId,
        GroupName = g.GroupName,
        TopicId = g.TopicId,
        TopicName = g.Topic?.TopicName ?? string.Empty,
        SupervisorId = g.SupervisorId,
        SupervisorName = g.Supervisor?.FullName ?? string.Empty,
        CreatedAt = g.CreatedAt,
        Members = g.Students.Select(s => new StudentDto
        {
            StudentId = s.UserId,
            UserCode = s.UserCode,
            FullName = s.FullName,
            Email = s.Email
        })
    };
}
