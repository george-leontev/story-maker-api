using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Comment;
using StoryMakerApi.Models;

namespace StoryMakerApi.Services;

public interface ICommentService
{
    Task<Result<CommentResponse>> CreateAsync(int storyId, CreateCommentRequest request, User user, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommentResponse>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int commentId, int userId, CancellationToken cancellationToken);
}
