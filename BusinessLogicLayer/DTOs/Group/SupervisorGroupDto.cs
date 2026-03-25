namespace BusinessLogicLayer.DTOs.Group;

public class SupervisorGroupDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}
