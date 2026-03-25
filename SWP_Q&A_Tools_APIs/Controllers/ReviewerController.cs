using System.Security.Claims;
using BusinessLogicLayer.DTOs.Question;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWP_Q_A_Tools_APIs.Controllers;

[ApiController]
[Route("api/reviewer/questions")]
[Authorize(Roles = "Reviewer")]
public class ReviewerController : ControllerBase
{
    private readonly IReviewerService _reviewerService;

    public ReviewerController(IReviewerService reviewerService)
    {
        _reviewerService = reviewerService;
    }

    // GET /api/reviewer/questions?status=Approved&search=abc
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionListDto>>> GetMyQuestions(
        [FromQuery] string? status,
        [FromQuery] Guid? topic_id,
        [FromQuery] string? search)
    {
        var reviewerId = GetCurrentUserId();
        if (reviewerId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var result = await _reviewerService.GetMyQuestionsAsync(reviewerId.Value, status, topic_id, search);
        return Ok(result);
    }

    // GET /api/reviewer/questions/:id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionDetailDto>> GetQuestionDetail(Guid id)
    {
        var reviewerId = GetCurrentUserId();
        if (reviewerId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var result = await _reviewerService.GetQuestionDetailAsync(reviewerId.Value, id);
        if (result is null)
            return NotFound(new { message = "Question not found." });

        return Ok(result);
    }

    // POST /api/reviewer/questions/:id/answer
    [HttpPost("{id:guid}/answer")]
    public async Task<ActionResult<QuestionDetailDto>> Answer(Guid id, [FromBody] AnswerQuestionRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var reviewerId = GetCurrentUserId();
        if (reviewerId is null)
            return Unauthorized(new { message = "Cannot identify current user." });

        var (result, error, statusCode) = await _reviewerService.AnswerQuestionAsync(reviewerId.Value, id, request.AnswerContent);

        if (error is not null)
        {
            return statusCode switch
            {
                404 => NotFound(new { message = error }),
                409 => Conflict(new { message = error }),
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
