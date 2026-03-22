namespace BusinessLogicLayer.DTOs.Group;

public class GroupDetailDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; }
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    public Guid SupervisorId { get; set; }
    public string SupervisorName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public IEnumerable<StudentDto> Members { get; set; }
}
