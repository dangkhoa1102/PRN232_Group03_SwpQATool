using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces;

public interface IHistoryRepository
{
    Task<(IEnumerable<HistoryRecord> Data, IEnumerable<(string Status, int Count)> StatusCounts)> GetHistoryAsync(
        string? status,
        Guid? topicId,
        Guid? groupId,
        DateTime? fromDate,
        DateTime? toDate,
        string? search);
}
