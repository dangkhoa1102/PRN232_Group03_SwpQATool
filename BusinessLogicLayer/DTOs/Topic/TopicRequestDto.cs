using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs.Topic;

public class TopicRequestDto
{
    [Required(ErrorMessage = "topic_name is required.")]
    [MaxLength(100, ErrorMessage = "topic_name must not exceed 100 characters.")]
    public string TopicName { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "reviewer_id is required.")]
    public Guid ReviewerId { get; set; }
}
