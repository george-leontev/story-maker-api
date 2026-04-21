using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class RatingRepository : IRatingRepository
{
    private readonly LivePlotDbContext _db;

    public RatingRepository(LivePlotDbContext db) => _db = db;

    public async Task<StoryRating?> FindByUserAndStoryAsync(int userId, int storyId, CancellationToken cancellationToken)
    {
        return await _db.StoryRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.StoryId == storyId, cancellationToken);
    }

    public async Task AddAsync(StoryRating rating, CancellationToken cancellationToken)
    {
        if (_db.Entry(rating).State == EntityState.Detached)
        {
            _db.StoryRatings.Add(rating);
        }
        
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<float> GetAverageAsync(int storyId, CancellationToken cancellationToken)
    {
        return await _db.StoryRatings
            .AsNoTracking()
            .Where(r => r.StoryId == storyId)
            .AverageAsync(r => (float?)r.Score, cancellationToken) ?? 0f;
    }

    public async Task<int> GetCountAsync(int storyId, CancellationToken cancellationToken)
    {
        return await _db.StoryRatings
            .AsNoTracking()
            .CountAsync(r => r.StoryId == storyId, cancellationToken);
    }

    public async Task DeleteAsync(StoryRating rating, CancellationToken cancellationToken)
    {
        _db.StoryRatings.Remove(rating);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
