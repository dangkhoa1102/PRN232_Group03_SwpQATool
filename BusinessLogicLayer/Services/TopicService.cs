using BusinessLogicLayer.DTOs.Topic;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Services;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly IUserRepository _userRepository;

    public TopicService(ITopicRepository topicRepository, IUserRepository userRepository)
    {
        _topicRepository = topicRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<TopicDto>> GetAllAsync()
    {
        var topics = await _topicRepository.GetAllAsync();
        return topics.Select(MapToDto);
    }

    public async Task<TopicDto?> GetByIdAsync(Guid id)
    {
        var topic = await _topicRepository.GetByIdAsync(id);
        return topic is null ? null : MapToDto(topic);
    }

    public async Task<(TopicDto? Result, string? Error)> CreateAsync(TopicRequestDto request)
    {
        var error = await ValidateAsync(request);
        if (error is not null)
            return (null, error);

        if (await _topicRepository.ExistsByNameAsync(request.TopicName))
            return (null, $"A topic with the name '{request.TopicName}' already exists.");

        var topic = new Topic
        {
            TopicId = Guid.NewGuid(),
            TopicName = request.TopicName.Trim(),
            Description = request.Description?.Trim(),
            ReviewerId = request.ReviewerId,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
        };

        var created = await _topicRepository.CreateAsync(topic);
        var withReviewer = await _topicRepository.GetByIdAsync(created.TopicId);
        return (MapToDto(withReviewer!), null);
    }

    public async Task<(TopicDto? Result, string? Error)> UpdateAsync(Guid id, TopicRequestDto request)
    {
        var topic = await _topicRepository.GetByIdAsync(id);
        if (topic is null)
            return (null, "Topic not found.");

        var error = await ValidateAsync(request);
        if (error is not null)
            return (null, error);

        if (await _topicRepository.ExistsByNameAsync(request.TopicName, excludeId: id))
            return (null, $"A topic with the name '{request.TopicName}' already exists.");

        topic.TopicName = request.TopicName.Trim();
        topic.Description = request.Description?.Trim();
        topic.ReviewerId = request.ReviewerId;

        var updated = await _topicRepository.UpdateAsync(topic);
        var withReviewer = await _topicRepository.GetByIdAsync(updated.TopicId);
        return (MapToDto(withReviewer!), null);
    }

    public async Task<string?> DeleteAsync(Guid id)
    {
        var topic = await _topicRepository.GetByIdAsync(id);
        if (topic is null)
            return "Topic not found.";

        if (await _topicRepository.IsUsedInGroupsAsync(id))
            return "Cannot delete topic because it is currently assigned to one or more groups.";

        await _topicRepository.DeleteAsync(topic);
        return null;
    }

    // BR-02: reviewer_id must refer to a user with role = 'Reviewer'
    private Task<string?> ValidateAsync(TopicRequestDto request)
    {
        var reviewer = _userRepository.GetById(request.ReviewerId);
        if (reviewer is null)
            return Task.FromResult<string?>("reviewer_id does not correspond to any existing user.");

        if (!string.Equals(reviewer.Role, "Reviewer", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>($"User '{reviewer.FullName}' does not have the Reviewer role.");

        return Task.FromResult<string?>(null);
    }

    private static TopicDto MapToDto(Topic topic) => new()
    {
        TopicId = topic.TopicId,
        TopicName = topic.TopicName,
        Description = topic.Description,
        ReviewerId = topic.ReviewerId,
        ReviewerName = topic.Reviewer?.FullName ?? string.Empty,
        ReviewerEmail = topic.Reviewer?.Email ?? string.Empty,
        CreatedAt = topic.CreatedAt
    };
}
