using BusinessLogicLayer.DTOs.Question;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Services;

public class ReviewerService : IReviewerService
{
    private const string Approved = "Approved";
    private const string Answered = "Answered";

    private readonly IQuestionWorkflowRepository _questionRepository;

    public ReviewerService(IQuestionWorkflowRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public async Task<IEnumerable<QuestionListDto>> GetMyQuestionsAsync(Guid reviewerId, string? status, Guid? topicId, string? search)
    {
        var questions = await _questionRepository.GetReviewerQuestionsAsync(reviewerId, status, topicId, search);
        return questions.Select(MapToListDto);
    }

    public async Task<QuestionDetailDto?> GetQuestionDetailAsync(Guid reviewerId, Guid questionId)
    {
        var question = await _questionRepository.GetReviewerQuestionByIdAsync(reviewerId, questionId);
        return question is null ? null : MapToDetailDto(question);
    }

    public async Task<(QuestionDetailDto? Result, string? Error, int StatusCode)> AnswerQuestionAsync(Guid reviewerId, Guid questionId, string answerContent)
    {
        if (string.IsNullOrWhiteSpace(answerContent))
            return (null, "answer_content is required.", 400);

        var question = await _questionRepository.GetReviewerQuestionByIdAsync(reviewerId, questionId);
        if (question is null)
            return (null, "Question not found.", 404);

        if (!string.Equals(question.Status, Approved, StringComparison.OrdinalIgnoreCase))
            return (null, "Only questions in 'Approved' status can be answered.", 400);

        if (await _questionRepository.HasAnswerAsync(questionId))
            return (null, "Question already has an official answer.", 409);

        var now = NowSeAsia();

        var answer = new Answer
        {
            AnswerId = Guid.NewGuid(),
            QuestionId = question.QuestionId,
            ReviewerId = reviewerId,
            AnswerContent = answerContent.Trim(),
            AnsweredAt = now
        };

        await _questionRepository.AddAnswerAsync(answer);

        question.Status = Answered;
        question.UpdatedAt = now;
        await _questionRepository.UpdateQuestionAsync(question);

        var notifications = new List<Notification>
        {
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = question.CreatedBy,
                Message = $"Your question '{question.Title}' has been answered.",
                IsRead = false,
                CreatedAt = now
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = question.Group.SupervisorId,
                Message = $"Question '{question.Title}' from your group has been answered.",
                IsRead = false,
                CreatedAt = now
            }
        };

        var distinctNotifications = notifications
            .GroupBy(n => n.UserId)
            .Select(g => g.First())
            .ToList();

        await _questionRepository.AddNotificationsAsync(distinctNotifications);

        var updated = await _questionRepository.GetReviewerQuestionByIdAsync(reviewerId, questionId);
        return (MapToDetailDto(updated!), null, 200);
    }

    private static DateTime NowSeAsia() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

    private static QuestionListDto MapToListDto(Question q) => new()
    {
        QuestionId = q.QuestionId,
        Title = q.Title,
        Status = q.Status,
        GroupName = q.Group.GroupName,
        TopicName = q.Topic.TopicName,
        CreatedByName = q.CreatedByNavigation.FullName,
        CreatedAt = q.CreatedAt
    };

    private static QuestionDetailDto MapToDetailDto(Question q) => new()
    {
        QuestionId = q.QuestionId,
        Title = q.Title,
        Content = q.Content,
        Status = q.Status,
        RejectReason = q.RejectReason,
        CreatedById = q.CreatedBy,
        CreatedByName = q.CreatedByNavigation.FullName,
        GroupId = q.GroupId,
        GroupName = q.Group.GroupName,
        TopicId = q.TopicId,
        TopicName = q.Topic.TopicName,
        ApprovedBy = q.ApprovedBy,
        ApprovedByName = q.ApprovedByNavigation?.FullName,
        ApprovedAt = q.ApprovedAt,
        CreatedAt = q.CreatedAt,
        UpdatedAt = q.UpdatedAt,
        Answer = q.Answer is null
            ? null
            : new AnswerInfoDto
            {
                AnswerId = q.Answer.AnswerId,
                AnswerContent = q.Answer.AnswerContent,
                ReviewerId = q.Answer.ReviewerId,
                ReviewerName = q.Answer.Reviewer.FullName,
                AnsweredAt = q.Answer.AnsweredAt
            }
    };
}
