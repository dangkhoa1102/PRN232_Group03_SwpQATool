using BusinessLogicLayer.DTOs.Topic;

namespace BusinessLogicLayer.Services.Interfaces;

public interface ITopicService
{
    Task<IEnumerable<TopicDto>> GetAllAsync();
    Task<TopicDto?> GetByIdAsync(Guid id);
    Task<(TopicDto? Result, string? Error)> CreateAsync(TopicRequestDto request);
    Task<(TopicDto? Result, string? Error)> UpdateAsync(Guid id, TopicRequestDto request);
    Task<string?> DeleteAsync(Guid id);
}
