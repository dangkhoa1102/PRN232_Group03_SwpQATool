using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class QuestionWorkflowRepository : IQuestionWorkflowRepository
{
    private readonly swp_qa_toolsContext _context;

    public QuestionWorkflowRepository(swp_qa_toolsContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Question>> GetSupervisorQuestionsAsync(Guid supervisorId, string? status, Guid? topicId, string? search)
    {
        var query = BuildQuestionQuery()
            .Where(q => q.Group.SupervisorId == supervisorId);

        query = ApplyFilters(query, status, topicId, search);

        return await query
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetSupervisorQuestionsByGroupAsync(Guid supervisorId, Guid groupId, string? status, string? search)
    {
        var query = BuildQuestionQuery()
            .Where(q => q.Group.SupervisorId == supervisorId && q.GroupId == groupId);

        query = ApplyFilters(query, status, topicId: null, search);

        return await query
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Question?> GetSupervisorQuestionByIdAsync(Guid supervisorId, Guid questionId)
    {
        return await BuildQuestionQuery()
            .Where(q => q.Group.SupervisorId == supervisorId)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId);
    }

    public async Task<IEnumerable<Question>> GetReviewerQuestionsAsync(Guid reviewerId, string? status, Guid? topicId, string? search)
    {
        var query = BuildQuestionQuery()
            .Where(q => q.Topic.ReviewerId == reviewerId);

        query = ApplyFilters(query, status, topicId, search);

        return await query
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Question?> GetReviewerQuestionByIdAsync(Guid reviewerId, Guid questionId)
    {
        return await BuildQuestionQuery()
            .Where(q => q.Topic.ReviewerId == reviewerId)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId);
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasAnswerAsync(Guid questionId)
    {
        return await _context.Answers.AnyAsync(a => a.QuestionId == questionId);
    }

    public async Task AddAnswerAsync(Answer answer)
    {
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();
    }

    public async Task AddNotificationsAsync(IEnumerable<Notification> notifications)
    {
        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Question> BuildQuestionQuery()
    {
        return _context.Questions
            .Include(q => q.CreatedByNavigation)
            .Include(q => q.ApprovedByNavigation)
            .Include(q => q.Group)
            .Include(q => q.Topic)
            .Include(q => q.Answer)
                .ThenInclude(a => a.Reviewer)
            .AsQueryable();
    }

    private static IQueryable<Question> ApplyFilters(IQueryable<Question> query, string? status, Guid? topicId, string? search)
    {
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(q => q.Status == status);

        if (topicId.HasValue)
            query = query.Where(q => q.TopicId == topicId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(q => q.Title.Contains(search) || q.Content.Contains(search));

        return query;
    }
}
