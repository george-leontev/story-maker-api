using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Rating;

namespace StoryMakerApi.Services;

public interface IRatingService
{
    Task<Result<RatingResponse>> RateAsync(int storyId, int userId, int score, CancellationToken cancellationToken);
    Task<Result> RemoveRatingAsync(int storyId, int userId, CancellationToken cancellationToken);
    Task<RatingResponse> GetRatingAsync(int storyId, int? userId, CancellationToken cancellationToken);
}
