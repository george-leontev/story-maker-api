using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class VoteRepository : IVoteRepository
{
    private readonly LivePlotDbContext _db;

    public VoteRepository(LivePlotDbContext db) => _db = db;

    public async Task<PagedResult<Vote>> GetVotesByUserAsync(int userId, int skip, int take, CancellationToken cancellationToken)
    {
        var votes = await _db.Votes
            .Include(v => v.Choice)
                .ThenInclude(c => c.Chapter)
                    .ThenInclude(ch => ch.Story)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.Choice.ExpiresAt) // Сортировка по дате закрытия выбора
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        var totalCount = await _db.Votes.CountAsync(v => v.UserId == userId, cancellationToken);

        return new PagedResult<Vote>(votes, totalCount);
    }

    public async Task<bool> HasVotedAsync(int choiceId, int userId, CancellationToken cancellationToken)
    {
        return await _db.Votes
            .AnyAsync(v => v.ChoiceId == choiceId && v.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Vote>> GetByChoiceAsync(int choiceId, CancellationToken cancellationToken)
    {
        return await _db.Votes
            .Where(v => v.ChoiceId == choiceId)
            .ToListAsync(cancellationToken);
    }
}