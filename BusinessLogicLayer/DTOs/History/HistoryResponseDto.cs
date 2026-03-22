namespace BusinessLogicLayer.DTOs.History;

public class HistoryResponseDto
{
    public IEnumerable<HistoryItemDto> Data { get; set; } = [];
    public HistorySummaryDto Summary { get; set; } = new();
}
