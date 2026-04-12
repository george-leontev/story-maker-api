using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Subscription;
using StoryMakerApi.Models;

namespace StoryMakerApi.Services;

public interface ISubscriptionService
{
    Task<Result> SubscribeAsync(int storyId, int userId, CancellationToken cancellationToken);
    Task<Result> UnsubscribeAsync(int storyId, int userId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<SubscriptionResponse>>> GetSubscribersAsync(int storyId, int authorId, CancellationToken cancellationToken);
}
