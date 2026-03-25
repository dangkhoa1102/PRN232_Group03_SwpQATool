using BusinessLogicLayer.DTOs.Question;
using BusinessLogicLayer.DTOs.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<QuestionListDto>> GetMyQuestions(Guid userId, string? status = null, string? keyword = null, int page = 1, int pageSize = 10);
        Task<QuestionDetailDto?> GetQuestionDetail(Guid questionId, Guid userId);
        Task<Guid> CreateQuestion(Guid userId, string title, string content);
        Task<bool> UpdateRejectedQuestion(Guid userId, Guid questionId, string title, string content);
        Task<List<NotificationDto>> GetNotifications(Guid userId, bool? isRead = null);
        Task<bool> MarkNotificationAsRead(Guid notificationId, Guid userId);
    }
}