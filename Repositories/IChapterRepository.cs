using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface IChapterRepository
{
    Task<Chapter?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Chapter> Items, int TotalCount)> GetByStoryIdAsync(int storyId, int skip, int take, CancellationToken cancellationToken);
    Task AddAsync(Chapter chapter, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> HasChoiceAsync(int chapterId, CancellationToken cancellationToken);
    Task<bool> ExistsByStoryAndSequenceAsync(int storyId, int sequenceNumber, CancellationToken cancellationToken);
    Task<int> GetMaxSequenceNumberAsync(int storyId, CancellationToken cancellationToken);
}
