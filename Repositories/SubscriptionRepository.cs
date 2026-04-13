using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly LivePlotDbContext _db;

    public SubscriptionRepository(LivePlotDbContext db) => _db = db;

    public async Task<bool> IsSubscribedAsync(int userId, int storyId, CancellationToken cancellationToken)
    {
        return await _db.Subscriptions
            .AsNoTracking()
            .AnyAsync(s => s.UserId == userId && s.StoryId == storyId, cancellationToken);
    }

    public async Task SubscribeAsync(int userId, int storyId, CancellationToken cancellationToken)
    {
        var subscription = new Subscription { UserId = userId, StoryId = storyId };
        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UnsubscribeAsync(int userId, int storyId, CancellationToken cancellationToken)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.StoryId == storyId, cancellationToken);

        if (subscription is not null)
        {
            _db.Subscriptions.Remove(subscription);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<Subscription>> GetSubscribersAsync(int storyId, CancellationToken cancellationToken)
    {
        return await _db.Subscriptions
            .Include(s => s.User)
            .Where(s => s.StoryId == storyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Story> Items, int TotalCount)> GetSubscribedStoriesAsync(int userId, int skip, int take, CancellationToken cancellationToken)
    {
        var query = _db.Subscriptions
            .Include(s => s.Story)
                .ThenInclude(story => story.Author)
            .Include(s => s.Story)
                .ThenInclude(story => story.Chapters)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Story.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).Select(s => s.Story!).ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
