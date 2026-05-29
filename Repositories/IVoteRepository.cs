using StoryMakerApi.Dtos.User;
using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface IVoteRepository
{
    Task<PagedResult<Vote>> GetVotesByUserAsync(int userId, int skip, int take, CancellationToken cancellationToken);
    Task<bool> HasVotedAsync(int choiceId, int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Vote>> GetByChoiceAsync(int choiceId, CancellationToken cancellationToken);
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }

    public PagedResult(IReadOnlyList<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}