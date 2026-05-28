namespace StoryMakerApi.Dtos.User;

public sealed record StoryListItem(
    int Id,
    string Title,
    string? CoverImageUrl,
    DateTime CreatedAt,
    int ChaptersCount,
    float Rating);