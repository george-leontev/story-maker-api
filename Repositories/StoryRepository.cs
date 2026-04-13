using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class StoryRepository : IStoryRepository
{
    private readonly LivePlotDbContext _db;

    public StoryRepository(LivePlotDbContext db) => _db = db;

    public async Task<Story?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Stories
            .Include(s => s.Author)
            .Include(s => s.Chapters)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Story?> FindByIdForUpdateAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Stories
            .Include(s => s.Author)
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Story> Items, int TotalCount)> GetAllAsync(int skip, int take, CancellationToken cancellationToken)
    {
        var query = _db.Stories
            .Include(s => s.Author)
            .Include(s => s.Chapters)
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Story> Items, int TotalCount)> GetByAuthorAsync(int authorId, int skip, int take, CancellationToken cancellationToken)
    {
        var query = _db.Stories
            .Include(s => s.Author)
            .Include(s => s.Chapters)
            .AsNoTracking()
            .Where(s => s.AuthorId == authorId)
            .OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Story story, CancellationToken cancellationToken)
    {
        _db.Stories.Add(story);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Story story, CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsAuthorAsync(int storyId, int userId, CancellationToken cancellationToken)
    {
        return await _db.Stories
            .AsNoTracking()
            .AnyAsync(s => s.Id == storyId && s.AuthorId == userId, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var story = await _db.Stories.FindAsync([id], cancellationToken: cancellationToken);
        if (story != null)
        {
            _db.Stories.Remove(story);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddChoiceAsync(Choice choice, CancellationToken cancellationToken)
    {
        _db.Choices.Add(choice);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
