using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Story;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class StoryService : IStoryService
{
    private readonly IStoryRepository _storyRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly ILogger<StoryService> _logger;
    private readonly IWebHostEnvironment _env;

    public StoryService(IStoryRepository storyRepository, IRatingRepository ratingRepository, ILogger<StoryService> logger, IWebHostEnvironment env)
    {
        _storyRepository = storyRepository;
        _ratingRepository = ratingRepository;
        _logger = logger;
        _env = env;
    }

    public async Task<Result<StoryResponse>> CreateAsync(CreateStoryRequest request, User author, IFormFile? coverImage = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<StoryResponse>.Failure("Название обязательно.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Result<StoryResponse>.Failure("Описание обязательно.");

        string? coverImageUrl = null;
        if (coverImage != null)
        {
            coverImageUrl = await SaveCoverImageAsync(coverImage, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            coverImageUrl = request.CoverImageUrl;
        }

        var story = new Story
        {
            AuthorId = author.Id,
            Title = request.Title,
            Description = request.Description,
            CoverImageUrl = coverImageUrl,
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
            return Result<StoryResponse>.Failure("История не найдена.");

        var avgRating = await _ratingRepository.GetAverageAsync(id, cancellationToken);
        story.Rating = avgRating;

        return Result<StoryResponse>.Success(MapToResponse(story, story.Author!.Username));
    }

    public async Task<PagedResponse<StoryResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (page - 1) * pageSize;
        var (items, totalCount) = await _storyRepository.GetAllAsync(skip, pageSize, cancellationToken);

        var stories = items.Select(s => MapToResponse(s, s.Author!.Username)).ToList();
        return new PagedResponse<StoryResponse>(stories.AsReadOnly(), totalCount, page, pageSize);
    }

    public async Task<PagedResponse<StoryResponse>> GetByAuthorAsync(int authorId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (page - 1) * pageSize;
        var (items, totalCount) = await _storyRepository.GetByAuthorAsync(authorId, skip, pageSize, cancellationToken);

        var stories = items.Select(s => MapToResponse(s, s.Author!.Username)).ToList();
        return new PagedResponse<StoryResponse>(stories.AsReadOnly(), totalCount, page, pageSize);
    }

    public async Task<Result<StoryResponse>> UpdateAsync(int id, UpdateStoryRequest request, User author, IFormFile? coverImage = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<StoryResponse>.Failure("Название обязательно.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Result<StoryResponse>.Failure("Описание обязательно.");

        var isAuthor = await _storyRepository.IsAuthorAsync(id, author.Id, cancellationToken);
        if (!isAuthor)
            return Result<StoryResponse>.Failure("Вы не являетесь автором этой истории.");

        var story = await _storyRepository.FindByIdForUpdateAsync(id, cancellationToken);
        if (story == null)
            return Result<StoryResponse>.Failure("История не найдена.");

        string? coverImageUrl = story.CoverImageUrl;
        if (coverImage != null)
        {
            // Delete old cover if exists
            if (!string.IsNullOrWhiteSpace(story.CoverImageUrl))
            {
                var oldCoverPath = Path.Combine(_env.ContentRootPath, story.CoverImageUrl.TrimStart('/'));
                if (File.Exists(oldCoverPath))
                {
                    File.Delete(oldCoverPath);
                }
            }
            coverImageUrl = await SaveCoverImageAsync(coverImage, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            coverImageUrl = request.CoverImageUrl;
        }

        story.Title = request.Title;
        story.Description = request.Description;
        story.CoverImageUrl = coverImageUrl;
        await _storyRepository.UpdateAsync(story, cancellationToken);

        _logger.LogInformation("Story updated: {StoryId} by {AuthorId}", id, author.Id);

        return Result<StoryResponse>.Success(MapToResponse(story, story.Author!.Username));
    }

    public async Task<Result<bool>> DeleteAsync(int id, User author, CancellationToken cancellationToken)
    {
        var isAuthor = await _storyRepository.IsAuthorAsync(id, author.Id, cancellationToken);
        if (!isAuthor)
            return Result<bool>.Failure("Вы не являетесь автором этой истории.");

        var story = await _storyRepository.FindByIdForUpdateAsync(id, cancellationToken);
        if (story == null)
            return Result<bool>.Failure("История не найдена.");

        // Delete cover image file if exists
        if (!string.IsNullOrWhiteSpace(story.CoverImageUrl))
        {
            var coverPath = Path.Combine(_env.ContentRootPath, story.CoverImageUrl.TrimStart('/'));
            if (File.Exists(coverPath))
            {
                File.Delete(coverPath);
            }
        }

        await _storyRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Story deleted: {StoryId} by {AuthorId}", id, author.Id);

        return Result<bool>.Success(true);
    }

    private async Task<string> SaveCoverImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException("Разрешены только изображения: JPG, PNG, WEBP");

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_env.ContentRootPath, "data", "covers", fileName);

        // Ensure directory exists
        Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "data", "covers"));

        await using var stream = new FileStream(filePath, FileMode.Create);
        await image.CopyToAsync(stream, cancellationToken);

        return $"/data/covers/{fileName}";
    }

    private static StoryResponse MapToResponse(Story story, string authorUsername)
    {
        // Rating is computed from StoryRatings table, not from the Story.Rating field
        // For list responses, we use the cached Story.Rating; for GetById, we compute live
        return new StoryResponse(
            story.Id,
            story.Title,
            story.Description,
            story.CoverImageUrl,
            authorUsername,
            story.Rating,
            story.CreatedAt,
            story.Chapters?.Count ?? 0);
    }
}
