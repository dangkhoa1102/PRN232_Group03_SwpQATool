namespace BusinessLogicLayer.DTOs.History;

public class HistorySummaryDto
{
    public int Total { get; set; }
    public int Answered { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Pending { get; set; }
}
