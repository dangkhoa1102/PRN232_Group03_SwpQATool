using BusinessLogicLayer.DTOs.Group;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWP_Q_A_Tools_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    // GET /api/groups
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetAll()
    {
        var groups = await _groupService.GetAllAsync();
        return Ok(groups);
    }

    // GET /api/groups/:id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupDetailDto>> GetById(Guid id)
    {
        var group = await _groupService.GetByIdAsync(id);
        if (group is null)
            return NotFound(new { message = "Group not found." });

        return Ok(group);
    }

    // POST /api/groups
    [HttpPost]
    public async Task<ActionResult<GroupDto>> Create([FromBody] GroupRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, error) = await _groupService.CreateAsync(request);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.GroupId }, result);
    }

    // PUT /api/groups/:id
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GroupDto>> Update(Guid id, [FromBody] GroupRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, error) = await _groupService.UpdateAsync(id, request);
        if (error == "Group not found.")
            return NotFound(new { message = error });

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(result);
    }

    // DELETE /api/groups/:id
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var error = await _groupService.DeleteAsync(id);
        if (error == "Group not found.")
            return NotFound(new { message = error });

        if (error is not null)
            return BadRequest(new { message = error });

        return NoContent();
    }

    // POST /api/groups/:id/members
    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var error = await _groupService.AddMemberAsync(id, request);
        if (error == "Group not found.")
            return NotFound(new { message = error });

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(new { message = "Member added successfully." });
    }

    // DELETE /api/groups/:id/members/:studentId
    [HttpDelete("{id:guid}/members/{studentId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid studentId)
    {
        var error = await _groupService.RemoveMemberAsync(id, studentId);
        if (error == "Group not found.")
            return NotFound(new { message = error });

        if (error is not null)
            return BadRequest(new { message = error });

        return NoContent();
    }
}
