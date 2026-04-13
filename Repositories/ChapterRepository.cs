using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class ChapterRepository : IChapterRepository
{
    private readonly LivePlotDbContext _db;

    public ChapterRepository(LivePlotDbContext db) => _db = db;

    public async Task<Chapter?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Chapters
            .Include(c => c.Choice)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Chapter> Items, int TotalCount)> GetByStoryIdAsync(int storyId, int skip, int take, CancellationToken cancellationToken)
    {
        var query = _db.Chapters
            .Include(c => c.Choice)
            .AsNoTracking()
            .Where(c => c.StoryId == storyId)
            .OrderBy(c => c.SequenceNumber);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken)
    {
        _db.Chapters.Add(chapter);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var chapter = await _db.Chapters.FindAsync([id], cancellationToken: cancellationToken);
        if (chapter != null)
        {
            _db.Chapters.Remove(chapter);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> HasChoiceAsync(int chapterId, CancellationToken cancellationToken)
    {
        return await _db.Choices
            .AsNoTracking()
            .AnyAsync(c => c.ChapterId == chapterId, cancellationToken);
    }

    public async Task<bool> ExistsByStoryAndSequenceAsync(int storyId, int sequenceNumber, CancellationToken cancellationToken)
    {
        return await _db.Chapters
            .AsNoTracking()
            .AnyAsync(c => c.StoryId == storyId && c.SequenceNumber == sequenceNumber, cancellationToken);
    }

    public async Task<int> GetMaxSequenceNumberAsync(int storyId, CancellationToken cancellationToken)
    {
        return await _db.Chapters
            .AsNoTracking()
            .Where(c => c.StoryId == storyId)
            .MaxAsync(c => (int?)c.SequenceNumber, cancellationToken) ?? 0;
    }
}
