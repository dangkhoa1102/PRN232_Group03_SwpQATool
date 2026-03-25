using BusinessLogicLayer.DTOs.Question;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SWP_Q_A_Tools_APIs.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet("questions")]
        public async Task<IActionResult> GetMyQuestions([FromQuery] string? status = null, [FromQuery] string? keyword = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (!Guid.TryParse(GetUserId(), out var userGuid))
                return Unauthorized();
            var result = await _studentService.GetMyQuestions(userGuid, status, keyword, page, pageSize);
            return Ok(result);
        }

        [HttpGet("questions/{id:guid}")]
        public async Task<IActionResult> GetQuestionDetail(Guid id)
        {
            if (!Guid.TryParse(GetUserId(), out var userGuid))
                return Unauthorized();
            var result = await _studentService.GetQuestionDetail(id, userGuid);
            if (result == null)
                return NotFound(new { message = "Question not found." });
            return Ok(result);
        }

        [HttpPost("questions")]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title must not be empty.");
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Content must not be empty.");
            if (!Guid.TryParse(GetUserId(), out var userGuid))
                return Unauthorized();
            var questionId = await _studentService.CreateQuestion(userGuid, dto.Title, dto.Content);
            if (questionId == Guid.Empty)
                return BadRequest("Student is not assigned to any group.");
            return Ok(new { questionId });
        }

        [HttpPut("questions/{id:guid}")]
        public async Task<IActionResult> UpdateRejectedQuestion(Guid id, [FromBody] UpdateQuestionRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title must not be empty.");
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Content must not be empty.");
            if (!Guid.TryParse(GetUserId(), out var userGuid))
                return Unauthorized();
            var success = await _studentService.UpdateRejectedQuestion(userGuid, id, dto.Title, dto.Content);
            if (!success) return BadRequest("Only rejected questions can be edited, or you do not have permission.");
            return Ok();
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] bool? isRead = null)
        {
            if (!Guid.TryParse(GetUserId(), out var userGuid))
                return Unauthorized();
            var result = await _studentService.GetNotifications(userGuid, isRead);
            return Ok(result);
        }

        [HttpPost("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid id)
        {

            if (!Guid.TryParse(GetUserId(), out var userGuid))
                return Unauthorized();

            var success = await _studentService.MarkNotificationAsRead(id, userGuid);

            if (!success) return BadRequest("Notification not found or not yours.");
            return Ok();
       }
    }

    public class CreateQuestionRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
    public class UpdateQuestionRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
