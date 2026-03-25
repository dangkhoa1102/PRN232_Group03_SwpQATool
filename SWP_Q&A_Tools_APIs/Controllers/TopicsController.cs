using BusinessLogicLayer.DTOs.Topic;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWP_Q_A_Tools_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    // GET /api/topics
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TopicDto>>> GetAll()
    {
        var topics = await _topicService.GetAllAsync();
        return Ok(topics);
    }

    // GET /api/topics/:id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TopicDto>> GetById(Guid id)
    {
        var topic = await _topicService.GetByIdAsync(id);
        if (topic is null)
            return NotFound(new { message = "Topic not found." });

        return Ok(topic);
    }

    // POST /api/topics
    [HttpPost]
    public async Task<ActionResult<TopicDto>> Create([FromBody] TopicRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, error) = await _topicService.CreateAsync(request);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.TopicId }, result);
    }

    // PUT /api/topics/:id
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TopicDto>> Update(Guid id, [FromBody] TopicRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, error) = await _topicService.UpdateAsync(id, request);
        if (error == "Topic not found.")
            return NotFound(new { message = error });

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(result);
    }

    // DELETE /api/topics/:id
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var error = await _topicService.DeleteAsync(id);
        if (error == "Topic not found.")
            return NotFound(new { message = error });

        if (error is not null)
            return BadRequest(new { message = error });

        return NoContent();
    }
}
