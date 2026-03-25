using System.Security.Claims;
using BusinessLogicLayer.DTOs.Group;
using BusinessLogicLayer.DTOs.Question;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWP_Q_A_Tools_APIs.Controllers;

[ApiController]
[Route("api/supervisor/questions")]
[Authorize(Roles = "Supervisor")]
public class SupervisorController : ControllerBase
{
    private readonly ISupervisorService _supervisorService;

    public SupervisorController(ISupervisorService supervisorService)
    {
        _supervisorService = supervisorService;
    }

    // GET /api/supervisor/questions/groups/me
    [HttpGet("groups/me")]
    public async Task<ActionResult<IEnumerable<SupervisorGroupDto>>> GetMyGroups()
    {
        var supervisorId = GetCurrentUserId();
        if (supervisorId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var result = await _supervisorService.GetMyGroupsAsync(supervisorId.Value);
        return Ok(result);
    }

    // GET /api/supervisor/questions/groups/:groupId/questions?status=Pending Approval&search=abc
    [HttpGet("groups/{groupId:guid}/questions")]
    public async Task<ActionResult<IEnumerable<QuestionListDto>>> GetQuestionsByGroup(
        Guid groupId,
        [FromQuery] string? status,
        [FromQuery] string? search)
    {
        var supervisorId = GetCurrentUserId();
        if (supervisorId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var (result, error, statusCode) = await _supervisorService.GetQuestionsByGroupAsync(supervisorId.Value, groupId, status, search);

        if (error is not null)
        {
            return statusCode switch
            {
                404 => NotFound(new { message = error }),
                _ => BadRequest(new { message = error })
            };
        }

        return Ok(result);
    }

    // GET /api/supervisor/questions?status=Pending Approval&search=abc
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionListDto>>> GetMyQuestions(
        [FromQuery] string? status,
        [FromQuery] Guid? topic_id,
        [FromQuery] string? search)
    {
        var supervisorId = GetCurrentUserId();
        if (supervisorId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var result = await _supervisorService.GetMyQuestionsAsync(supervisorId.Value, status, topic_id, search);
        return Ok(result);
    }

    // GET /api/supervisor/questions/:id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionDetailDto>> GetQuestionDetail(Guid id)
    {
        var supervisorId = GetCurrentUserId();
        if (supervisorId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var result = await _supervisorService.GetQuestionDetailAsync(supervisorId.Value, id);
        if (result is null)
            return NotFound(new { message = "Question not found." });

        return Ok(result);
    }

    // POST /api/supervisor/questions/:id/approve
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<QuestionDetailDto>> Approve(Guid id)
    {
        var supervisorId = GetCurrentUserId();
        if (supervisorId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var (result, error, statusCode) = await _supervisorService.ApproveQuestionAsync(supervisorId.Value, id);

        if (error is not null)
        {
            return statusCode switch
            {
                404 => NotFound(new { message = error }),
                _ => BadRequest(new { message = error })
            };
        }

        return Ok(result);
    }

    // POST /api/supervisor/questions/:id/reject
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<QuestionDetailDto>> Reject(Guid id, [FromBody] RejectQuestionRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var supervisorId = GetCurrentUserId();
        if (supervisorId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var (result, error, statusCode) = await _supervisorService.RejectQuestionAsync(supervisorId.Value, id, request.RejectReason);

        if (error is not null)
        {
            return statusCode switch
            {
                404 => NotFound(new { message = error }),
                _ => BadRequest(new { message = error })
            };
        }

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(idString, out var id) ? id : null;
    }
}
