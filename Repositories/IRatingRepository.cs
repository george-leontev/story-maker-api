using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface IRatingRepository
{
    Task<StoryRating?> FindByUserAndStoryAsync(int userId, int storyId, CancellationToken cancellationToken);
    Task AddAsync(StoryRating rating, CancellationToken cancellationToken);
    Task DeleteAsync(StoryRating rating, CancellationToken cancellationToken);
    Task<float> GetAverageAsync(int storyId, CancellationToken cancellationToken);
    Task<int> GetCountAsync(int storyId, CancellationToken cancellationToken);
}
