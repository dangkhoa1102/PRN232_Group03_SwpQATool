using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class HistoryRepository : IHistoryRepository
{
    private readonly swp_qa_toolsContext _context;

    public HistoryRepository(swp_qa_toolsContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<HistoryRecord> Data, IEnumerable<(string Status, int Count)> StatusCounts)> GetHistoryAsync(
        string? status,
        Guid? topicId,
        Guid? groupId,
        DateTime? fromDate,
        DateTime? toDate,
        string? search)
    {
        // Base filter — reused for both data and summary
        var baseQuery = _context.Questions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            baseQuery = baseQuery.Where(q => q.Status == status);

        if (topicId.HasValue)
            baseQuery = baseQuery.Where(q => q.TopicId == topicId.Value);

        if (groupId.HasValue)
            baseQuery = baseQuery.Where(q => q.GroupId == groupId.Value);

        if (fromDate.HasValue)
            baseQuery = baseQuery.Where(q => q.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            baseQuery = baseQuery.Where(q => q.CreatedAt <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(search))
            baseQuery = baseQuery.Where(q => q.Title.Contains(search));

        // Data query: join all related tables
        var dataQuery =
            from q in baseQuery
            join uCreate in _context.Users on q.CreatedBy equals uCreate.UserId
            join g in _context.Groups on q.GroupId equals g.GroupId
            join t in _context.Topics on q.TopicId equals t.TopicId
            join uApprove in _context.Users on q.ApprovedBy equals uApprove.UserId into approveJoin
            from uApprove in approveJoin.DefaultIfEmpty()
            join a in _context.Answers on q.QuestionId equals a.QuestionId into answerJoin
            from a in answerJoin.DefaultIfEmpty()
            join uReview in _context.Users on a.ReviewerId equals uReview.UserId into reviewJoin
            from uReview in reviewJoin.DefaultIfEmpty()
            orderby q.CreatedAt descending
            select new HistoryRecord
            {
                QuestionId = q.QuestionId,
                Title = q.Title,
                Status = q.Status,
                RejectReason = q.RejectReason,
                GroupName = g.GroupName,
                TopicName = t.TopicName,
                CreatedByName = uCreate.FullName,
                CreatedByCode = uCreate.UserCode,
                CreatedAt = q.CreatedAt,
                ApprovedByName = uApprove != null ? uApprove.FullName : null,
                ApprovedAt = q.ApprovedAt,
                AnsweredByName = uReview != null ? uReview.FullName : null,
                AnsweredAt = a != null ? a.AnsweredAt : null,
                ProcessingMinutes = a != null && a.AnsweredAt.HasValue
                    ? (int?)EF.Functions.DateDiffMinute(q.CreatedAt!.Value, a.AnsweredAt.Value)
                    : q.ApprovedAt.HasValue
                        ? (int?)EF.Functions.DateDiffMinute(q.CreatedAt!.Value, q.ApprovedAt.Value)
                        : null
            };

        var data = await dataQuery.ToListAsync();

        // Summary query: COUNT GROUP BY status (separate query per spec)
        var counts = await baseQuery
            .GroupBy(q => q.Status)
            .Select(grp => new { Status = grp.Key, Count = grp.Count() })
            .ToListAsync();

        var statusCounts = counts.Select(c => (c.Status, c.Count));

        return (data, statusCounts);
    }
}
