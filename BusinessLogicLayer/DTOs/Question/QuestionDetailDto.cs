namespace BusinessLogicLayer.DTOs.Question;

public class QuestionDetailDto
{
    public Guid QuestionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public AnswerInfoDto? Answer { get; set; }
}
