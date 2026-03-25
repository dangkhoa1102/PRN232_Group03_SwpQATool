namespace BusinessLogicLayer.DTOs.Topic;

public class TopicDto
{
    public Guid TopicId { get; set; }
    public string TopicName { get; set; } = null!;
    public string? Description { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = null!;
    public string ReviewerEmail { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}
