using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken);
    Task AddAsync(Comment comment, CancellationToken cancellationToken);
    Task<bool> IsAuthorAsync(int commentId, int userId, CancellationToken cancellationToken);
    Task DeleteAsync(int commentId, CancellationToken cancellationToken);
}
