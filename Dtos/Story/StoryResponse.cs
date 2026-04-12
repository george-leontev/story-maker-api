namespace StoryMakerApi.Dtos.Story;

public sealed record StoryResponse(
    int Id,
    string Title,
    string Description,
    string AuthorUsername,
    float Rating,
    DateTime CreatedAt,
    int ChapterCount
);
