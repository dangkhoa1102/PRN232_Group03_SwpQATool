namespace BusinessLogicLayer.DTOs.Group;

public class GroupDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; }
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    public Guid SupervisorId { get; set; }
    public string SupervisorName { get; set; }
    public int MemberCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
