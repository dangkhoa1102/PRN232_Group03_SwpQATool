namespace BusinessLogicLayer.DTOs.Group;

public class GroupDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public Guid TopicId { get; set; }
    public string TopicName { get; set; } = null!;
    public Guid SupervisorId { get; set; }
    public string SupervisorName { get; set; } = null!;
    public int MemberCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
