using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs.Group;

public class AddMemberDto
{
    [Required(ErrorMessage = "student_id is required.")]
    public Guid StudentId { get; set; }
}
