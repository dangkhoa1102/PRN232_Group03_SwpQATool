using BusinessLogicLayer.DTOs.History;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Services;

public class HistoryService : IHistoryService
{
    private static readonly HashSet<string> ValidStatuses =
        ["Pending Approval", "Approved", "Answered", "Rejected"];

    private readonly IHistoryRepository _historyRepository;

    public HistoryService(IHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<(HistoryResponseDto? Result, string? Error)> GetHistoryAsync(
        string? status,
        Guid? topicId,
        Guid? groupId,
        string? fromDate,
        string? toDate,
        string? search)
    {
        if (!string.IsNullOrWhiteSpace(status) && !ValidStatuses.Contains(status))
            return (null, $"status không hợp lệ. Chỉ chấp nhận: {string.Join(", ", ValidStatuses)}.");

        DateTime? from = null;
        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            if (!DateTime.TryParse(fromDate, out var parsed))
                return (null, "from_date không đúng định dạng ISO (ví dụ: 2025-01-01).");
            from = parsed.Date;
        }

        DateTime? to = null;
        if (!string.IsNullOrWhiteSpace(toDate))
        {
            if (!DateTime.TryParse(toDate, out var parsed))
                return (null, "to_date không đúng định dạng ISO (ví dụ: 2025-01-31).");
            to = parsed.Date.AddDays(1).AddTicks(-1);
        }

        var (data, statusCounts) = await _historyRepository.GetHistoryAsync(
            status, topicId, groupId, from, to, search);

        var counts = statusCounts.ToList();

        var summary = new HistorySummaryDto
        {
            Total    = counts.Sum(c => c.Count),
            Answered = counts.FirstOrDefault(c => c.Status == "Answered").Count,
            Approved = counts.FirstOrDefault(c => c.Status == "Approved").Count,
            Rejected = counts.FirstOrDefault(c => c.Status == "Rejected").Count,
            Pending  = counts.FirstOrDefault(c => c.Status == "Pending Approval").Count,
        };

        var result = new HistoryResponseDto
        {
            Data = data.Select(MapToDto),
            Summary = summary
        };

        return (result, null);
    }

    private static HistoryItemDto MapToDto(HistoryRecord r) => new()
    {
        QuestionId      = r.QuestionId,
        Title           = r.Title,
        Status          = r.Status,
        RejectReason    = r.RejectReason,
        GroupName       = r.GroupName,
        TopicName       = r.TopicName,
        CreatedByName   = r.CreatedByName,
        CreatedByCode   = r.CreatedByCode,
        CreatedAt       = r.CreatedAt,
        ApprovedByName  = r.ApprovedByName,
        ApprovedAt      = r.ApprovedAt,
        AnsweredByName  = r.AnsweredByName,
        AnsweredAt      = r.AnsweredAt,
        ProcessingMinutes = r.ProcessingMinutes
    };
}
