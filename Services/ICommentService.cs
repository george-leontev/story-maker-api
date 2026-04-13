using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Comment;
using StoryMakerApi.Models;

namespace StoryMakerApi.Services;

public interface ICommentService
{
    Task<Result<CommentResponse>> CreateAsync(int storyId, CreateCommentRequest request, User user, CancellationToken cancellationToken);
    Task<PagedResponse<CommentResponse>> GetByStoryIdAsync(int storyId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int commentId, int userId, CancellationToken cancellationToken);
}
