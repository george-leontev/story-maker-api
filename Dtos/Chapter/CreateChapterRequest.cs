namespace StoryMakerApi.Dtos.Chapter;

public sealed record CreateChapterRequest(string Content, int? SequenceNumber = null);
