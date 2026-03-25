namespace DataAccessLayer.Models;

public class GroupMember
{
    public Guid GroupId { get; set; }
    public Guid StudentId { get; set; }

    public virtual Group Group { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
}