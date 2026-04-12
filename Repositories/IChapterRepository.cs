using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface IChapterRepository
{
    Task<Chapter?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Chapter>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken);
    Task AddAsync(Chapter chapter, CancellationToken cancellationToken);
    Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> HasChoiceAsync(int chapterId, CancellationToken cancellationToken);
}
