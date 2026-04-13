namespace StoryMakerApi.Dtos.Rating;

public sealed record RatingResponse(int StoryId, float AverageRating, int VoteCount, int? UserScore);
