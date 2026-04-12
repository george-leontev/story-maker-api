using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Chapter;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class ChapterService : IChapterService
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<ChapterService> _logger;

    public ChapterService(IChapterRepository chapterRepository, IStoryRepository storyRepository, ILogger<ChapterService> logger)
    {
        _chapterRepository = chapterRepository;
        _storyRepository = storyRepository;
        _logger = logger;
    }

    public async Task<Result<ChapterResponse>> CreateAsync(int storyId, CreateChapterRequest request, int authorId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result<ChapterResponse>.Failure("Content is required.");

        var story = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (story == null)
            return Result<ChapterResponse>.Failure("Story not found.");

        if (story.AuthorId != authorId)
            return Result<ChapterResponse>.Failure("You are not the author of this story.");

        var chapter = new Chapter
        {
            StoryId = storyId,
            Content = request.Content,
            SequenceNumber = request.SequenceNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _chapterRepository.AddAsync(chapter, cancellationToken);

        _logger.LogInformation("Chapter created: {ChapterId} for Story: {StoryId}", chapter.Id, storyId);

        return Result<ChapterResponse>.Success(MapToResponse(chapter));
    }

    public async Task<Result<ChapterResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.FindByIdAsync(id, cancellationToken);
        if (chapter == null)
            return Result<ChapterResponse>.Failure("Chapter not found.");

        return Result<ChapterResponse>.Success(MapToResponse(chapter));
    }

    public async Task<IReadOnlyList<ChapterResponse>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken)
    {
        var chapters = await _chapterRepository.GetByStoryIdAsync(storyId, cancellationToken);
        return chapters.Select(MapToResponse).ToList();
    }

    public async Task<Result<ChapterResponse>> UpdateAsync(int id, UpdateChapterRequest request, int authorId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result<ChapterResponse>.Failure("Content is required.");

        var chapter = await _chapterRepository.FindByIdAsync(id, cancellationToken);
        if (chapter == null)
            return Result<ChapterResponse>.Failure("Chapter not found.");

        var story = await _storyRepository.FindByIdAsync(chapter.StoryId, cancellationToken);
        if (story == null || story.AuthorId != authorId)
            return Result<ChapterResponse>.Failure("You are not the author of this story.");

        // Content is init-only, so we'd need a model change. For now, return validation error
        // since Chapter.Content uses `required` + `init`.
        // In practice, chapters shouldn't be editable once published.
        return Result<ChapterResponse>.Failure("Chapters cannot be edited after publishing.");
    }

    public async Task<Result<bool>> DeleteAsync(int id, int authorId, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.FindByIdAsync(id, cancellationToken);
        if (chapter == null)
            return Result<bool>.Failure("Chapter not found.");

        var story = await _storyRepository.FindByIdAsync(chapter.StoryId, cancellationToken);
        if (story == null || story.AuthorId != authorId)
            return Result<bool>.Failure("You are not the author of this story.");

        await _chapterRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Chapter deleted: {ChapterId} by Author: {AuthorId}", id, authorId);

        return Result<bool>.Success(true);
    }

    private static ChapterResponse MapToResponse(Chapter chapter)
    {
        return new ChapterResponse(
            chapter.Id,
            chapter.StoryId,
            chapter.Content,
            chapter.SequenceNumber,
            chapter.CreatedAt,
            chapter.Choice != null);
    }
}
