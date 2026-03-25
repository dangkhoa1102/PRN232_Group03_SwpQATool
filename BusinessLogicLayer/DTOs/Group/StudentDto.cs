namespace BusinessLogicLayer.DTOs.Group;

public class StudentDto
{
    public Guid StudentId { get; set; }
    public string UserCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}
