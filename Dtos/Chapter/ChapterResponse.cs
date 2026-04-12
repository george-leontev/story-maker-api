namespace StoryMakerApi.Dtos.Chapter;

public sealed record ChapterResponse(
    int Id,
    int StoryId,
    string Content,
    int SequenceNumber,
    DateTime CreatedAt,
    bool HasChoice
);
