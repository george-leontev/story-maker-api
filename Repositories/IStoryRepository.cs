using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface IStoryRepository
{
    Task<Story?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<Story?> FindByIdForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Story> Items, int TotalCount)> GetAllAsync(int skip, int take, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Story> Items, int TotalCount)> GetByAuthorAsync(int authorId, int skip, int take, CancellationToken cancellationToken);
    Task AddAsync(Story story, CancellationToken cancellationToken);
    Task UpdateAsync(Story story, CancellationToken cancellationToken);
    Task<bool> IsAuthorAsync(int storyId, int userId, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task AddChoiceAsync(Choice choice, CancellationToken cancellationToken);
}
