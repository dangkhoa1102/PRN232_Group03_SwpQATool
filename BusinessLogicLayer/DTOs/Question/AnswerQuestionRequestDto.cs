using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs.Question;

public class AnswerQuestionRequestDto
{
    [Required(ErrorMessage = "answer_content is required.")]
    [MinLength(5, ErrorMessage = "answer_content must be at least 5 characters.")]
    public string AnswerContent { get; set; } = string.Empty;
}
