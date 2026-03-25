using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IGroupMemberRepository
    {
        Task<GroupMember> GetByStudentIdAsync(Guid studentId);
    }
}