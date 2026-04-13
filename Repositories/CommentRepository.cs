using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class CommentRepository : ICommentRepository
{
    private readonly LivePlotDbContext _db;

    public CommentRepository(LivePlotDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByStoryIdAsync(int storyId, int skip, int take, CancellationToken cancellationToken)
    {
        var query = _db.Comments
            .Include(c => c.User)
            .Where(c => c.StoryId == storyId)
            .OrderByDescending(c => c.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsAuthorAsync(int commentId, int userId, CancellationToken cancellationToken)
    {
        return await _db.Comments
            .AsNoTracking()
            .AnyAsync(c => c.Id == commentId && c.UserId == userId, cancellationToken);
    }

    public async Task DeleteAsync(int commentId, CancellationToken cancellationToken)
    {
        var comment = await _db.Comments.FindAsync([commentId], cancellationToken: cancellationToken);
        if (comment is not null)
        {
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
