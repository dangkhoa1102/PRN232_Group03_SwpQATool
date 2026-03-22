using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs.User;

public class UpdateUserRequestDto
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    public string? Role { get; set; }

    [MinLength(8)]
    public string? Password { get; set; }
}
