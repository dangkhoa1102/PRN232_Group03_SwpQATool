using BusinessLogicLayer.DTOs.Question;
using BusinessLogicLayer.DTOs.History;
using BusinessLogicLayer.DTOs.User;
using BusinessLogicLayer.DTOs.Group;
using BusinessLogicLayer.DTOs.Topic;
using BusinessLogicLayer.DTOs.Auth;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogicLayer.DTOs.Notification;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class StudentService : IStudentService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IGroupMemberRepository _groupMemberRepository;

        public StudentService(
            IQuestionRepository questionRepository,
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            INotificationRepository notificationRepository,
            ITopicRepository topicRepository,
            IGroupMemberRepository groupMemberRepository)
        {
            _questionRepository = questionRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _notificationRepository = notificationRepository;
            _topicRepository = topicRepository;
            _groupMemberRepository = groupMemberRepository;
        }

        public async Task<IEnumerable<QuestionListDto>> GetMyQuestions(Guid userId, string? status = null, string? keyword = null, int page = 1, int pageSize = 10)
        {
            var groupMember = await _groupMemberRepository.GetByStudentIdAsync(userId);
            if (groupMember == null) return Enumerable.Empty<QuestionListDto>();
            var groupId = groupMember.GroupId;
            var query = _questionRepository.Query()
                .Where(q => q.GroupId == groupId);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(q => q.Status == status);
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(q => q.Title.Contains(keyword));
            var questions = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(q => q.CreatedByNavigation)
                .Include(q => q.Group)
                .Include(q => q.Topic)
                .ToListAsync();
            return questions.Select(q => new QuestionListDto
            {
                QuestionId = q.QuestionId,
                Title = q.Title,
                Status = q.Status,
                GroupName = q.Group?.GroupName ?? string.Empty,
                TopicName = q.Topic?.TopicName ?? string.Empty,
                CreatedByName = q.CreatedByNavigation?.FullName ?? string.Empty,
                CreatedAt = q.CreatedAt
            });
        }

        public async Task<QuestionDetailDto?> GetQuestionDetail(Guid questionId, Guid userId)
        {
            var groupMember = await _groupMemberRepository.GetByStudentIdAsync(userId);
            if (groupMember == null) return null;
            var question = await _questionRepository.Query()
                .Where(q => q.QuestionId == questionId && q.GroupId == groupMember.GroupId)
                .Include(q => q.Answer)
                .ThenInclude(a => a.Reviewer)
                .Include(q => q.CreatedByNavigation)
                .Include(q => q.ApprovedByNavigation)
                .Include(q => q.Group)
                .Include(q => q.Topic)
                .FirstOrDefaultAsync();
            if (question == null) return null;
            return new QuestionDetailDto
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                Content = question.Content,
                Status = question.Status,
                RejectReason = question.RejectReason,
                CreatedById = question.CreatedBy,
                CreatedByName = question.CreatedByNavigation?.FullName ?? string.Empty,
                GroupId = question.GroupId,
                GroupName = question.Group?.GroupName ?? string.Empty,
                TopicId = question.TopicId,
                TopicName = question.Topic?.TopicName ?? string.Empty,
                ApprovedBy = question.ApprovedBy,
                ApprovedByName = question.ApprovedByNavigation?.FullName,
                ApprovedAt = question.ApprovedAt,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt,
                Answer = question.Answer != null ? new AnswerInfoDto
                {
                    AnswerId = question.Answer.AnswerId,
                    AnswerContent = question.Answer.AnswerContent,
                    ReviewerId = question.Answer.ReviewerId,
                    ReviewerName = question.Answer.Reviewer?.FullName ?? string.Empty,
                    AnsweredAt = question.Answer.AnsweredAt
                } : null
            };
        }

        public async Task<Guid> CreateQuestion(Guid userId, string title, string content)
        {
            var groupMember = await _groupMemberRepository.GetByStudentIdAsync(userId);
            if (groupMember == null) return Guid.Empty;
            var group = groupMember.Group;
            if (group == null) return Guid.Empty;
            var question = new DataAccessLayer.Models.Question
            {
                QuestionId = Guid.NewGuid(),
                Title = title,
                Content = content,
                Status = "Pending Approval",
                CreatedBy = userId,
                GroupId = group.GroupId,
                TopicId = group.TopicId,
                CreatedAt = DateTime.UtcNow
            };
            await _questionRepository.AddAsync(question);
            await _questionRepository.SaveChangesAsync();
            return question.QuestionId;
        }

        public async Task<bool> UpdateRejectedQuestion(Guid userId, Guid questionId, string title, string content)
        {
            var groupMember = await _groupMemberRepository.GetByStudentIdAsync(userId);
            if (groupMember == null) return false;
            var question = await _questionRepository.Query()
                .Where(q => q.QuestionId == questionId && q.GroupId == groupMember.GroupId)
                .FirstOrDefaultAsync();
            if (question == null) return false;
            if (question.Status != "Rejected") return false;
            question.Title = title;
            question.Content = content;
            question.Status = "Pending Approval";
            question.UpdatedAt = DateTime.UtcNow;
            await _questionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<NotificationDto>> GetNotifications(Guid userId, bool? isRead = null)
        {
            var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, isRead);
            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Message = n.Message,
                IsRead = n.IsRead ?? false,
                CreatedAt = n.CreatedAt ?? DateTime.UtcNow
            }).ToList();
        }

        public async Task<bool> MarkNotificationAsRead(Guid notificationId, Guid userId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        }
    }
}