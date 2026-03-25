using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class GroupMemberRepository : IGroupMemberRepository
    {
        private readonly swp_qa_toolsContext _context;
        public GroupMemberRepository(swp_qa_toolsContext context)
        {
            _context = context;
        }

        public async Task<GroupMember> GetByStudentIdAsync(Guid studentId)
        {
            return await _context.GroupMembers
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync(x => x.StudentId == studentId);
        }
    }
}