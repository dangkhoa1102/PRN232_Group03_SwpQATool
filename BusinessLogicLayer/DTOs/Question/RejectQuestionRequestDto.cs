using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs.Question;

public class RejectQuestionRequestDto
{
    [Required(ErrorMessage = "reject_reason is required.")]
    [MinLength(5, ErrorMessage = "reject_reason must be at least 5 characters.")]
    public string RejectReason { get; set; } = string.Empty;
}
