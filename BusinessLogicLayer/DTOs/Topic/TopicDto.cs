namespace BusinessLogicLayer.DTOs.Topic;

public class TopicDto
{
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    public string? Description { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; }
    public string ReviewerEmail { get; set; }
    public DateTime? CreatedAt { get; set; }
}
