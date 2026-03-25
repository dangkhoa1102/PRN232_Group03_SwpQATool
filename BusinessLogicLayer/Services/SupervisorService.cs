using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.DTOs.Group;
using BusinessLogicLayer.DTOs.Question;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Services;

public class SupervisorService : ISupervisorService
{
    private const string PendingApproval = "Pending Approval";
    private const string Approved = "Approved";
    private const string Rejected = "Rejected";

    private readonly IQuestionWorkflowRepository _questionRepository;
    private readonly IGroupRepository _groupRepository;

    public SupervisorService(IQuestionWorkflowRepository questionRepository, IGroupRepository groupRepository)
    {
        _questionRepository = questionRepository;
        _groupRepository = groupRepository;
    }

    public async Task<IEnumerable<SupervisorGroupDto>> GetMyGroupsAsync(Guid supervisorId)
    {
        var groups = await _groupRepository.GetBySupervisorIdAsync(supervisorId);
        return groups.Select(g => new SupervisorGroupDto
        {
            GroupId = g.GroupId,
            GroupName = g.GroupName,
            TopicId = g.TopicId,
            TopicName = g.Topic.TopicName,
            MemberCount = g.GroupMembers.Count
        });
    }

    public async Task<(IEnumerable<QuestionListDto>? Result, string? Error, int StatusCode)> GetQuestionsByGroupAsync(Guid supervisorId, Guid groupId, string? status, string? search)
    {
        var groupExists = await _groupRepository.ExistsByIdAndSupervisorIdAsync(groupId, supervisorId);
        if (!groupExists)
            return (null, "Group not found.", 404);

        var questions = await _questionRepository.GetSupervisorQuestionsByGroupAsync(supervisorId, groupId, status, search);
        return (questions.Select(MapToListDto), null, 200);
    }

    public async Task<IEnumerable<QuestionListDto>> GetMyQuestionsAsync(Guid supervisorId, string? status, Guid? topicId, string? search)
    {
        var questions = await _questionRepository.GetSupervisorQuestionsAsync(supervisorId, status, topicId, search);
        return questions.Select(MapToListDto);
    }

    public async Task<QuestionDetailDto?> GetQuestionDetailAsync(Guid supervisorId, Guid questionId)
    {
        var question = await _questionRepository.GetSupervisorQuestionByIdAsync(supervisorId, questionId);
        return question is null ? null : MapToDetailDto(question);
    }

    public async Task<(QuestionDetailDto? Result, string? Error, int StatusCode)> ApproveQuestionAsync(Guid supervisorId, Guid questionId)
    {
        var question = await _questionRepository.GetSupervisorQuestionByIdAsync(supervisorId, questionId);
        if (question is null)
            return (null, "Question not found.", 404);

        if (!string.Equals(question.Status, PendingApproval, StringComparison.OrdinalIgnoreCase))
            return (null, "Only questions in 'Pending Approval' status can be approved.", 400);

        var now = NowSeAsia();

        question.Status = Approved;
        question.RejectReason = null;
        question.ApprovedBy = supervisorId;
        question.ApprovedAt = now;
        question.UpdatedAt = now;

        await _questionRepository.UpdateQuestionAsync(question);

        var notifications = new List<Notification>
        {
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = question.CreatedBy,
                Message = $"Your question '{question.Title}' has been approved.",
                IsRead = false,
                CreatedAt = now
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = question.Topic.ReviewerId,
                Message = $"A new approved question in topic '{question.Topic.TopicName}' is waiting for your answer.",
                IsRead = false,
                CreatedAt = now
            }
        };

        await _questionRepository.AddNotificationsAsync(notifications);

        var updated = await _questionRepository.GetSupervisorQuestionByIdAsync(supervisorId, questionId);
        return (MapToDetailDto(updated!), null, 200);
    }

    public async Task<(QuestionDetailDto? Result, string? Error, int StatusCode)> RejectQuestionAsync(Guid supervisorId, Guid questionId, string rejectReason)
    {
        if (string.IsNullOrWhiteSpace(rejectReason))
            return (null, "reject_reason is required when status is Rejected.", 400);

        var question = await _questionRepository.GetSupervisorQuestionByIdAsync(supervisorId, questionId);
        if (question is null)
            return (null, "Question not found.", 404);

        if (!string.Equals(question.Status, PendingApproval, StringComparison.OrdinalIgnoreCase))
            return (null, "Only questions in 'Pending Approval' status can be rejected.", 400);

        var now = NowSeAsia();

        question.Status = Rejected;
        question.RejectReason = rejectReason.Trim();
        question.ApprovedBy = supervisorId;
        question.ApprovedAt = now;
        question.UpdatedAt = now;

        await _questionRepository.UpdateQuestionAsync(question);

        await _questionRepository.AddNotificationsAsync([
            new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = question.CreatedBy,
                Message = $"Your question '{question.Title}' has been rejected. Reason: {question.RejectReason}",
                IsRead = false,
                CreatedAt = now
            }
        ]);

        var updated = await _questionRepository.GetSupervisorQuestionByIdAsync(supervisorId, questionId);
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
