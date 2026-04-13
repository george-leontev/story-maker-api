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
            return Result<ChoiceResponse>.Failure("Текст первого варианта обязателен.");

        if (string.IsNullOrWhiteSpace(request.Option2Text))
            return Result<ChoiceResponse>.Failure("Текст второго варианта обязателен.");

        if (request.DurationInMinutes < 1)
            return Result<ChoiceResponse>.Failure("Длительность должна быть не менее 1 минуты.");

        var chapter = await _chapterRepository.FindByIdAsync(chapterId, cancellationToken);
        if (chapter == null)
            return Result<ChoiceResponse>.Failure("Глава не найдена.");

        var story = await _storyRepository.FindByIdAsync(chapter.StoryId, cancellationToken);
        if (story == null || story.AuthorId != authorId)
            return Result<ChoiceResponse>.Failure("Вы не являетесь автором этой истории.");

        if (await _chapterRepository.HasChoiceAsync(chapterId, cancellationToken))
            return Result<ChoiceResponse>.Failure("У этой главы уже есть выбор.");

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
            return Result<ChoiceResponse>.Failure("Выбор не найден.");

        await TryCloseExpiredChoiceAsync(choice, cancellationToken);

        return Result<ChoiceResponse>.Success(MapToPublicResponse(choice));
    }

    public async Task<Result<ChoiceAuthorResponse>> GetAuthorViewAsync(int id, int authorId, CancellationToken cancellationToken)
    {
        var choice = await _choiceRepository.FindByIdAsync(id, cancellationToken);
        if (choice == null)
            return Result<ChoiceAuthorResponse>.Failure("Выбор не найден.");

        var story = await _storyRepository.FindByIdAsync(choice.Chapter.StoryId, cancellationToken);
        if (story == null || story.AuthorId != authorId)
            return Result<ChoiceAuthorResponse>.Failure("Вы не являетесь автором этой истории.");

        await TryCloseExpiredChoiceAsync(choice, cancellationToken);

        return Result<ChoiceAuthorResponse>.Success(MapToAuthorResponse(choice));
    }

    public async Task<Result<bool>> VoteAsync(int choiceId, int option, int userId, CancellationToken cancellationToken)
    {
        if (option != 1 && option != 2)
            return Result<bool>.Failure("Неверный вариант. Должен быть 1 или 2.");

        var choice = await _choiceRepository.FindByIdForUpdateAsync(choiceId, cancellationToken);
        if (choice == null)
            return Result<bool>.Failure("Выбор не найден.");

        if (choice.IsClosed)
            return Result<bool>.Failure("Этот выбор закрыт. Голосование больше недоступно.");

        if (DateTime.UtcNow > choice.ExpiresAt)
        {
            _logger.LogInformation("Choice {ChoiceId} expired. Auto-closing on vote attempt.", choiceId);
            await _choiceRepository.CloseChoiceAsync(choice, cancellationToken);
            return Result<bool>.Failure("Этот выбор истёк. Голос не засчитан.");
        }

        if (await _choiceRepository.HasVotedAsync(choiceId, userId, cancellationToken))
            return Result<bool>.Failure("Вы уже проголосовали в этом выборе.");

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
        int? winningOption = choice.IsClosed ? choice.WinningOption : null;

        return new ChoiceResponse(
            choice.Id,
            choice.ChapterId,
            choice.Option1Text,
            choice.Option2Text,
            choice.ExpiresAt,
            choice.IsClosed,
            winningOption,
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
            choice.WinningOption,
            choice.ExpiresAt,
            choice.IsClosed);
    }
}
