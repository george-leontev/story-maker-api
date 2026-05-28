using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly LivePlotDbContext _db;

    public UserRepository(LivePlotDbContext db) => _db = db;

    public async Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> FindByIdWithDetailsAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Users
            .Include(u => u.Stories)
            .Include(u => u.Votes)
            .Include(u => u.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync([id], cancellationToken);
        if (user != null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<(int StoriesCount, int VotesCount, int CommentsCount)> GetUserStatsAsync(int userId, CancellationToken cancellationToken)
    {
        var storiesCount = await _db.Stories.CountAsync(s => s.AuthorId == userId, cancellationToken);
        var votesCount = await _db.Votes.CountAsync(v => v.UserId == userId, cancellationToken);
        var commentsCount = await _db.Comments.CountAsync(c => c.UserId == userId, cancellationToken);
        return (storiesCount, votesCount, commentsCount);
    }
}
