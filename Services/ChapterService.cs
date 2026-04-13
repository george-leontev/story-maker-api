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
            return Result<ChapterResponse>.Failure("Содержимое главы обязательно.");

        var story = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (story == null)
            return Result<ChapterResponse>.Failure("История не найдена.");

        if (story.AuthorId != authorId)
            return Result<ChapterResponse>.Failure("Вы не являетесь автором этой истории.");

        var sequenceNumber = request.SequenceNumber;
        if (sequenceNumber == null)
        {
            var maxSeq = await _chapterRepository.GetMaxSequenceNumberAsync(storyId, cancellationToken);
            sequenceNumber = maxSeq + 1;
        }
        else
        {
            var duplicateExists = await _chapterRepository.ExistsByStoryAndSequenceAsync(storyId, sequenceNumber.Value, cancellationToken);
            if (duplicateExists)
                return Result<ChapterResponse>.Failure($"Глава с порядковым номером {sequenceNumber} уже существует в этой истории.");
        }

        var chapter = new Chapter
        {
            StoryId = storyId,
            Content = request.Content,
            SequenceNumber = sequenceNumber.Value,
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
            return Result<ChapterResponse>.Failure("Глава не найдена.");

        return Result<ChapterResponse>.Success(MapToResponse(chapter));
    }

    public async Task<PagedResponse<ChapterResponse>> GetByStoryIdAsync(int storyId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (page - 1) * pageSize;
        var (items, totalCount) = await _chapterRepository.GetByStoryIdAsync(storyId, skip, pageSize, cancellationToken);

        var chapters = items.Select(MapToResponse).ToList();
        return new PagedResponse<ChapterResponse>(chapters.AsReadOnly(), totalCount, page, pageSize);
    }

    public async Task<Result<bool>> DeleteAsync(int id, int authorId, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.FindByIdAsync(id, cancellationToken);
        if (chapter == null)
            return Result<bool>.Failure("Глава не найдена.");

        var story = await _storyRepository.FindByIdAsync(chapter.StoryId, cancellationToken);
        if (story == null || story.AuthorId != authorId)
            return Result<bool>.Failure("Вы не являетесь автором этой истории.");

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
