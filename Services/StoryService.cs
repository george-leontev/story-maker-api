using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Story;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class StoryService : IStoryService
{
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<StoryService> _logger;

    public StoryService(IStoryRepository storyRepository, ILogger<StoryService> logger)
    {
        _storyRepository = storyRepository;
        _logger = logger;
    }

    public async Task<Result<StoryResponse>> CreateAsync(CreateStoryRequest request, User author, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<StoryResponse>.Failure("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Result<StoryResponse>.Failure("Description is required.");

        var story = new Story
        {
            AuthorId = author.Id,
            Title = request.Title,
            Description = request.Description,
            Rating = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _storyRepository.AddAsync(story, cancellationToken);

        _logger.LogInformation("Story created: {StoryId} by {AuthorId}", story.Id, author.Id);

        return Result<StoryResponse>.Success(MapToResponse(story, author.Username));
    }

    public async Task<Result<StoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.FindByIdAsync(id, cancellationToken);

        if (story == null)
            return Result<StoryResponse>.Failure("Story not found.");

        return Result<StoryResponse>.Success(MapToResponse(story, story.Author!.Username));
    }

    public async Task<IReadOnlyList<StoryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var stories = await _storyRepository.GetAllAsync(cancellationToken);

        return stories.Select(s => MapToResponse(s, s.Author!.Username)).ToList();
    }

    public async Task<IReadOnlyList<StoryResponse>> GetByAuthorAsync(int authorId, CancellationToken cancellationToken)
    {
        var stories = await _storyRepository.GetByAuthorAsync(authorId, cancellationToken);

        return stories.Select(s => MapToResponse(s, s.Author!.Username)).ToList();
    }

    public async Task<Result<StoryResponse>> UpdateAsync(int id, UpdateStoryRequest request, User author, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<StoryResponse>.Failure("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Result<StoryResponse>.Failure("Description is required.");

        var isAuthor = await _storyRepository.IsAuthorAsync(id, author.Id, cancellationToken);
        if (!isAuthor)
            return Result<StoryResponse>.Failure("You are not the author of this story.");

        var story = await _storyRepository.FindByIdForUpdateAsync(id, cancellationToken);
        if (story == null)
            return Result<StoryResponse>.Failure("Story not found.");

        story.Title = request.Title;
        story.Description = request.Description;
        await _storyRepository.UpdateAsync(story, cancellationToken);

        _logger.LogInformation("Story updated: {StoryId} by {AuthorId}", id, author.Id);

        return Result<StoryResponse>.Success(MapToResponse(story, story.Author!.Username));
    }

    public async Task<Result<bool>> DeleteAsync(int id, User author, CancellationToken cancellationToken)
    {
        var isAuthor = await _storyRepository.IsAuthorAsync(id, author.Id, cancellationToken);
        if (!isAuthor)
            return Result<bool>.Failure("You are not the author of this story.");

        await _storyRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Story deleted: {StoryId} by {AuthorId}", id, author.Id);

        return Result<bool>.Success(true);
    }

    private static StoryResponse MapToResponse(Story story, string authorUsername)
    {
        return new StoryResponse(
            story.Id,
            story.Title,
            story.Description,
            authorUsername,
            story.Rating,
            story.CreatedAt,
            story.Chapters?.Count ?? 0);
    }
}
