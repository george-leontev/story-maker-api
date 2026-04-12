using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Comment;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IStoryRepository _storyRepository;

    public CommentService(ICommentRepository commentRepository, IStoryRepository storyRepository)
    {
        _commentRepository = commentRepository;
        _storyRepository = storyRepository;
    }

    public async Task<Result<CommentResponse>> CreateAsync(int storyId, CreateCommentRequest request, User user, CancellationToken cancellationToken)
    {
        var storyExists = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (storyExists is null)
            return Result<CommentResponse>.Failure("История не найдена.");

        if (string.IsNullOrWhiteSpace(request.Text))
            return Result<CommentResponse>.Failure("Текст комментария не может быть пустым.");

        var comment = new Comment
        {
            StoryId = storyId,
            UserId = user.Id,
            Text = request.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);

        return Result<CommentResponse>.Success(new CommentResponse(
            comment.Id,
            comment.StoryId,
            user.Username,
            comment.Text,
            comment.Timestamp
        ));
    }

    public async Task<IReadOnlyList<CommentResponse>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken)
    {
        var comments = await _commentRepository.GetByStoryIdAsync(storyId, cancellationToken);

        return comments
            .Select(c => new CommentResponse(c.Id, c.StoryId, c.User.Username, c.Text, c.Timestamp))
            .ToList()
            .AsReadOnly();
    }

    public async Task<Result<bool>> DeleteAsync(int commentId, int userId, CancellationToken cancellationToken)
    {
        var isAuthor = await _commentRepository.IsAuthorAsync(commentId, userId, cancellationToken);
        if (!isAuthor)
        {
            return Result<bool>.Failure("Вы не являетесь автором этого комментария.");
        }

        await _commentRepository.DeleteAsync(commentId, cancellationToken);
        return Result<bool>.Success(true);
    }
}
