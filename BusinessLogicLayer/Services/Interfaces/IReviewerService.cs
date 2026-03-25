using BusinessLogicLayer.DTOs.Question;

namespace BusinessLogicLayer.Services.Interfaces;

public interface IReviewerService
{
    Task<IEnumerable<QuestionListDto>> GetMyQuestionsAsync(Guid reviewerId, string? status, Guid? topicId, string? search);
    Task<QuestionDetailDto?> GetQuestionDetailAsync(Guid reviewerId, Guid questionId);
    Task<(QuestionDetailDto? Result, string? Error, int StatusCode)> AnswerQuestionAsync(Guid reviewerId, Guid questionId, string answerContent);
}
