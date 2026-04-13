using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Rating;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IStoryRepository _storyRepository;

    public RatingService(IRatingRepository ratingRepository, IStoryRepository storyRepository)
    {
        _ratingRepository = ratingRepository;
        _storyRepository = storyRepository;
    }

    public async Task<Result<RatingResponse>> RateAsync(int storyId, int userId, int score, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (story is null)
            return Result<RatingResponse>.Failure("История не найдена.");

        if (score < 1 || score > 5)
            return Result<RatingResponse>.Failure("Оценка должна быть от 1 до 5.");

        var existing = await _ratingRepository.FindByUserAndStoryAsync(userId, storyId, cancellationToken);
        if (existing is not null)
        {
            existing.Score = score;
            await _ratingRepository.AddAsync(existing, cancellationToken);
        }
        else
        {
            var rating = new StoryRating
            {
                StoryId = storyId,
                UserId = userId,
                Score = score,
                CreatedAt = DateTime.UtcNow
            };
            await _ratingRepository.AddAsync(rating, cancellationToken);
        }

        var avg = await _ratingRepository.GetAverageAsync(storyId, cancellationToken);
        var count = await _ratingRepository.GetCountAsync(storyId, cancellationToken);

        return Result<RatingResponse>.Success(new RatingResponse(storyId, avg, count, score));
    }

    public async Task<Result> RemoveRatingAsync(int storyId, int userId, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.FindByIdAsync(storyId, cancellationToken);
        if (story is null)
            return Result.Failure("История не найдена.");

        var existing = await _ratingRepository.FindByUserAndStoryAsync(userId, storyId, cancellationToken);
        if (existing is null)
            return Result.Failure("Вы не оценивали эту историю.");

        await _ratingRepository.DeleteAsync(existing, cancellationToken);
        return Result.Success();
    }

    public async Task<RatingResponse> GetRatingAsync(int storyId, int? userId, CancellationToken cancellationToken)
    {
        var avg = await _ratingRepository.GetAverageAsync(storyId, cancellationToken);
        var count = await _ratingRepository.GetCountAsync(storyId, cancellationToken);

        int? userScore = null;
        if (userId.HasValue)
        {
            var rating = await _ratingRepository.FindByUserAndStoryAsync(userId.Value, storyId, cancellationToken);
            userScore = rating?.Score;
        }

        return new RatingResponse(storyId, avg, count, userScore);
    }
}
