using BusinessLogicLayer.DTOs.Group;
using BusinessLogicLayer.DTOs.Question;

namespace BusinessLogicLayer.Services.Interfaces;

public interface ISupervisorService
{
    Task<IEnumerable<SupervisorGroupDto>> GetMyGroupsAsync(Guid supervisorId);
    Task<(IEnumerable<QuestionListDto>? Result, string? Error, int StatusCode)> GetQuestionsByGroupAsync(Guid supervisorId, Guid groupId, string? status, string? search);
    Task<IEnumerable<QuestionListDto>> GetMyQuestionsAsync(Guid supervisorId, string? status, Guid? topicId, string? search);
    Task<QuestionDetailDto?> GetQuestionDetailAsync(Guid supervisorId, Guid questionId);
    Task<(QuestionDetailDto? Result, string? Error, int StatusCode)> ApproveQuestionAsync(Guid supervisorId, Guid questionId);
    Task<(QuestionDetailDto? Result, string? Error, int StatusCode)> RejectQuestionAsync(Guid supervisorId, Guid questionId, string rejectReason);
}
