namespace BusinessLogicLayer.DTOs.Question;

public class QuestionListDto
{
    public Guid QuestionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}
