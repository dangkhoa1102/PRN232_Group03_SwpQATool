using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs.Group;

public class GroupRequestDto
{
    [Required(ErrorMessage = "group_name is required.")]
    [MaxLength(100, ErrorMessage = "group_name must not exceed 100 characters.")]
    public string GroupName { get; set; }

    [Required(ErrorMessage = "topic_id is required.")]
    public Guid TopicId { get; set; }

    [Required(ErrorMessage = "supervisor_id is required.")]
    public Guid SupervisorId { get; set; }
}
