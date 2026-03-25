using BusinessLogicLayer.DTOs.History;
using BusinessLogicLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWP_Q_A_Tools_APIs.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public AdminController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    // GET /api/admin/history
    [HttpGet("history")]
    public async Task<ActionResult<HistoryResponseDto>> GetHistory(
        [FromQuery] string? status,
        [FromQuery] Guid? topic_id,
        [FromQuery] Guid? group_id,
        [FromQuery] string? from_date,
        [FromQuery] string? to_date,
        [FromQuery] string? search)
    {
        var (result, error) = await _historyService.GetHistoryAsync(
            status, topic_id, group_id, from_date, to_date, search);

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(result);
    }
}
