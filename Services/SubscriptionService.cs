using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Subscription;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IStoryRepository _storyRepository;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, IStoryRepository storyRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _storyRepository = storyRepository;
    }

    public async Task<Result> SubscribeAsync(int storyId, int userId, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (story is null)
            return Result.Failure("История не найдена.");

        var alreadySubscribed = await _subscriptionRepository.IsSubscribedAsync(userId, storyId, cancellationToken);
        if (alreadySubscribed)
            return Result.Failure("Вы уже подписаны на эту историю.");

        await _subscriptionRepository.SubscribeAsync(userId, storyId, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UnsubscribeAsync(int storyId, int userId, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (story is null)
            return Result.Failure("История не найдена.");

        var isSubscribed = await _subscriptionRepository.IsSubscribedAsync(userId, storyId, cancellationToken);
        if (!isSubscribed)
            return Result.Failure("Вы не подписаны на эту историю.");

        await _subscriptionRepository.UnsubscribeAsync(userId, storyId, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<SubscriptionResponse>>> GetSubscribersAsync(int storyId, int authorId, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (story is null)
            return Result<IReadOnlyList<SubscriptionResponse>>.Failure("История не найдена.");

        if (story.AuthorId != authorId)
            return Result<IReadOnlyList<SubscriptionResponse>>.Failure("Только автор истории может просматривать подписчиков.");

        var subscriptions = await _subscriptionRepository.GetSubscribersAsync(storyId, cancellationToken);

        var subscribers = subscriptions
            .Select(s => new SubscriptionResponse(s.User.Id, s.User.Username, s.User.Email))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<SubscriptionResponse>>.Success(subscribers);
    }
}
