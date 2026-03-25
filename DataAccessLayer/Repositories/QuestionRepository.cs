using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly swp_qa_toolsContext _context;

        public QuestionRepository(swp_qa_toolsContext context)
        {
            _context = context;
        }

        public IQueryable<Question> Query()
        {
            return _context.Questions.AsQueryable();
        }

        public async Task AddAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}