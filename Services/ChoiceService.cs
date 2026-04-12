using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Choice;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class ChoiceService : IChoiceService
{
    private readonly IChoiceRepository _choiceRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<ChoiceService> _logger;

    public ChoiceService(
        IChoiceRepository choiceRepository,
        IChapterRepository chapterRepository,
        IStoryRepository storyRepository,
        ILogger<ChoiceService> logger)
    {
        _choiceRepository = choiceRepository;
        _chapterRepository = chapterRepository;
        _storyRepository = storyRepository;
        _logger = logger;
    }

    public async Task<Result<ChoiceResponse>> CreateAsync(int chapterId, CreateChoiceRequest request, int authorId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Option1Text))
            return Result<ChoiceResponse>.Failure("Option 1 text is required.");

        if (string.IsNullOrWhiteSpace(request.Option2Text))
            return Result<ChoiceResponse>.Failure("Option 2 text is required.");

        if (request.DurationInMinutes < 1)
            return Result<ChoiceResponse>.Failure("Duration must be at least 1 minute.");

        var chapter = await _chapterRepository.FindByIdAsync(chapterId, cancellationToken);
        if (chapter == null)
            return Result<ChoiceResponse>.Failure("Chapter not found.");

        var story = await _storyRepository.FindByIdAsync(chapter.StoryId, cancellationToken);
        if (story == null || story.AuthorId != authorId)
            return Result<ChoiceResponse>.Failure("You are not the author of this story.");

        if (await _chapterRepository.HasChoiceAsync(chapterId, cancellationToken))
            return Result<ChoiceResponse>.Failure("This chapter already has an active choice.");

        var choice = new Models.Choice
        {
            ChapterId = chapterId,
            Option1Text = request.Option1Text,
            Option2Text = request.Option2Text,
            Option1Votes = 0,
            Option2Votes = 0,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.DurationInMinutes),
            IsClosed = false
        };

        await _storyRepository.AddChoiceAsync(choice, cancellationToken);

        _logger.LogInformation("Choice created: {ChoiceId} for Chapter: {ChapterId}", choice.Id, chapterId);

        return Result<ChoiceResponse>.Success(MapToPublicResponse(choice));
    }

    public async Task<Result<ChoiceResponse>> GetPublicAsync(int id, CancellationToken cancellationToken)
    {
        var choice = await _choiceRepository.FindByIdAsync(id, cancellationToken);
        if (choice == null)
            return Result<ChoiceResponse>.Failure("Choice not found.");

        await TryCloseExpiredChoiceAsync(choice, cancellationToken);

        return Result<ChoiceResponse>.Success(MapToPublicResponse(choice));
    }

    public async Task<Result<ChoiceAuthorResponse>> GetAuthorViewAsync(int id, int authorId, CancellationToken cancellationToken)
    {
        var choice = await _choiceRepository.FindByIdAsync(id, cancellationToken);
        if (choice == null)
            return Result<ChoiceAuthorResponse>.Failure("Choice not found.");

        var story = await _storyRepository.FindByIdAsync(choice.Chapter.StoryId, cancellationToken);
        if (story == null || story.AuthorId != authorId)
            return Result<ChoiceAuthorResponse>.Failure("You are not the author of this story.");

        await TryCloseExpiredChoiceAsync(choice, cancellationToken);

        return Result<ChoiceAuthorResponse>.Success(MapToAuthorResponse(choice));
    }

    public async Task<Result<bool>> VoteAsync(int choiceId, int option, int userId, CancellationToken cancellationToken)
    {
        if (option != 1 && option != 2)
            return Result<bool>.Failure("Invalid option. Must be 1 or 2.");

        var choice = await _choiceRepository.FindByIdForUpdateAsync(choiceId, cancellationToken);
        if (choice == null)
            return Result<bool>.Failure("Choice not found.");

        if (choice.IsClosed)
            return Result<bool>.Failure("This choice is closed. Voting is no longer available.");

        if (DateTime.UtcNow > choice.ExpiresAt)
        {
            _logger.LogInformation("Choice {ChoiceId} expired. Auto-closing on vote attempt.", choiceId);
            await _choiceRepository.CloseChoiceAsync(choice, cancellationToken);
            return Result<bool>.Failure("This choice has expired. Vote not counted.");
        }

        if (await _choiceRepository.HasVotedAsync(choiceId, userId, cancellationToken))
            return Result<bool>.Failure("You have already voted on this choice.");

        await _choiceRepository.RecordVoteAsync(choice, option, userId, cancellationToken);

        _logger.LogInformation("Vote recorded: User {UserId} voted option {Option} on Choice {ChoiceId}", userId, option, choiceId);

        return Result<bool>.Success(true);
    }

    private async Task TryCloseExpiredChoiceAsync(Models.Choice choice, CancellationToken cancellationToken)
    {
        if (!choice.IsClosed && DateTime.UtcNow > choice.ExpiresAt)
        {
            await _choiceRepository.CloseChoiceAsync(choice, cancellationToken);
            _logger.LogInformation("Choice {ChoiceId} expired. Auto-closed during read.", choice.Id);
        }
    }

    private static ChoiceResponse MapToPublicResponse(Models.Choice choice)
    {
        int? option1Votes = choice.IsClosed ? choice.Option1Votes : null;
        int? option2Votes = choice.IsClosed ? choice.Option2Votes : null;

        return new ChoiceResponse(
            choice.Id,
            choice.ChapterId,
            choice.Option1Text,
            choice.Option2Text,
            choice.ExpiresAt,
            choice.IsClosed,
            option1Votes,
            option2Votes);
    }

    private static ChoiceAuthorResponse MapToAuthorResponse(Models.Choice choice)
    {
        return new ChoiceAuthorResponse(
            choice.Id,
            choice.ChapterId,
            choice.Option1Text,
            choice.Option2Text,
            choice.Option1Votes,
            choice.Option2Votes,
            choice.ExpiresAt,
            choice.IsClosed);
    }
}
