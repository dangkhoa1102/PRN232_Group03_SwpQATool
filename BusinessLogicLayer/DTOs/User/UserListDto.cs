namespace BusinessLogicLayer.DTOs.User;

public class UserListDto
{
    public Guid UserId { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}
