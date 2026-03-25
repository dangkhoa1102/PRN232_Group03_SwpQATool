using DataAccessLayer.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IQuestionRepository
    {
        IQueryable<Question> Query();
        Task AddAsync(Question question);
        Task SaveChangesAsync();
    }
}