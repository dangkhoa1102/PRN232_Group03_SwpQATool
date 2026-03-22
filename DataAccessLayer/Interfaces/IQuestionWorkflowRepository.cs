using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces;

public interface IQuestionWorkflowRepository
{
    Task<IEnumerable<Question>> GetSupervisorQuestionsAsync(Guid supervisorId, string? status, Guid? topicId, string? search);
    Task<IEnumerable<Question>> GetSupervisorQuestionsByGroupAsync(Guid supervisorId, Guid groupId, string? status, string? search);
    Task<Question?> GetSupervisorQuestionByIdAsync(Guid supervisorId, Guid questionId);

    Task<IEnumerable<Question>> GetReviewerQuestionsAsync(Guid reviewerId, string? status, Guid? topicId, string? search);
    Task<Question?> GetReviewerQuestionByIdAsync(Guid reviewerId, Guid questionId);

    Task UpdateQuestionAsync(Question question);
    Task<bool> HasAnswerAsync(Guid questionId);
    Task AddAnswerAsync(Answer answer);
    Task AddNotificationsAsync(IEnumerable<Notification> notifications);
}
