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

    public async Task<IReadOnlyList<Chapter>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken)
    {
        return await _db.Chapters
            .Include(c => c.Choice)
            .AsNoTracking()
            .Where(c => c.StoryId == storyId)
            .OrderBy(c => c.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken)
    {
        _db.Chapters.Add(chapter);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken)
    {
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
}
