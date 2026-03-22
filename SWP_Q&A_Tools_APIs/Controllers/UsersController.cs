using System.Security.Claims;
using BusinessLogicLayer.DTOs.User;
using BusinessLogicLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWP_Q_A_Tools_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /api/users?role=Student&unassigned=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserListDto>>> GetAll(
        [FromQuery] string? role,
        [FromQuery] bool unassigned = false)
    {
        var users = await _userService.GetAllAsync(role, unassigned);
        return Ok(users);
    }

    // GET /api/users/:id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserListDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { message = "Không tìm thấy user." });

        return Ok(user);
    }

    // POST /api/users
    [HttpPost]
    public async Task<ActionResult<UserListDto>> Create([FromBody] CreateUserRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, error, statusCode) = await _userService.CreateAsync(request);

        if (error is not null)
        {
            return statusCode switch
            {
                409 => Conflict(new { message = error }),
                _ => BadRequest(new { message = error })
            };
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.UserId }, result);
    }

    // PUT /api/users/:id
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserListDto>> Update(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, error, statusCode) = await _userService.UpdateAsync(id, request);

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

    // DELETE /api/users/:id
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = "Không xác định được user hiện tại." });

        var (error, statusCode) = await _userService.DeleteAsync(id, currentUserId.Value);

        if (error is not null)
        {
            return statusCode switch
            {
                404 => NotFound(new { message = error }),
                _ => BadRequest(new { message = error })
            };
        }

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(idString, out var id) ? id : null;
    }
}
