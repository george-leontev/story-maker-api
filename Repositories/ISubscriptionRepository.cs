using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface ISubscriptionRepository
{
    Task<bool> IsSubscribedAsync(int userId, int storyId, CancellationToken cancellationToken);
    Task SubscribeAsync(int userId, int storyId, CancellationToken cancellationToken);
    Task UnsubscribeAsync(int userId, int storyId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Subscription>> GetSubscribersAsync(int storyId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Story> Items, int TotalCount)> GetSubscribedStoriesAsync(int userId, int skip, int take, CancellationToken cancellationToken);
}
