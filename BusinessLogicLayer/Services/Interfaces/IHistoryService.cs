using BusinessLogicLayer.DTOs.History;

namespace BusinessLogicLayer.Services.Interfaces;

public interface IHistoryService
{
    Task<(HistoryResponseDto? Result, string? Error)> GetHistoryAsync(
        string? status,
        Guid? topicId,
        Guid? groupId,
        string? fromDate,
        string? toDate,
        string? search);
}
