namespace BusinessLogicLayer.DTOs.Question;

public class AnswerInfoDto
{
    public Guid AnswerId { get; set; }
    public string AnswerContent { get; set; } = string.Empty;
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public DateTime? AnsweredAt { get; set; }
}
