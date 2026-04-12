using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public sealed class ChoiceRepository : IChoiceRepository
{
    private readonly LivePlotDbContext _db;

    public ChoiceRepository(LivePlotDbContext db) => _db = db;

    public async Task<Choice?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Choices
            .Include(c => c.Chapter)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Choice?> FindByIdForUpdateAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Choices
            .Include(c => c.Chapter)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> HasVotedAsync(int choiceId, int userId, CancellationToken cancellationToken)
    {
        return await _db.Votes
            .AsNoTracking()
            .AnyAsync(v => v.ChoiceId == choiceId && v.UserId == userId, cancellationToken);
    }

    public async Task RecordVoteAsync(Choice choice, int option, int userId, CancellationToken cancellationToken)
    {
        if (option == 1)
            choice.Option1Votes++;
        else if (option == 2)
            choice.Option2Votes++;

        var vote = new Vote
        {
            UserId = userId,
            ChoiceId = choice.Id,
            SelectedOption = option
        };

        _db.Votes.Add(vote);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetVoteCountAsync(int choiceId, int option, CancellationToken cancellationToken)
    {
        var choice = await _db.Choices
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == choiceId, cancellationToken);

        return choice == null ? 0 : (option == 1 ? choice.Option1Votes : choice.Option2Votes);
    }

    public async Task CloseChoiceAsync(Choice choice, CancellationToken cancellationToken)
    {
        choice.IsClosed = true;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
