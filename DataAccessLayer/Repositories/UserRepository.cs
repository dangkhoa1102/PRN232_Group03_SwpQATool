using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories;

public class UserRepository : IUserRepository
{
    private readonly swp_qa_toolsContext _context;

    public UserRepository(swp_qa_toolsContext context)
    {
        _context = context;
    }

    public User? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
    }
}
