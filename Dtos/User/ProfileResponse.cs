namespace StoryMakerApi.Dtos.User;

public sealed record ProfileResponse(
    int Id,
    string Username,
    string Email,
    string? AvatarImageUrl,
    DateTime CreatedAt,
    int StoriesCount,
    int VotesCount,
    int CommentsCount);