using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces;

public interface IUserRepository
{
    User? GetByEmail(string email);
}
