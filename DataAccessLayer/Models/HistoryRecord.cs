namespace DataAccessLayer.Models;

public class HistoryRecord
{
    public Guid QuestionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public string CreatedByCode { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? AnsweredByName { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public int? ProcessingMinutes { get; set; }
}
