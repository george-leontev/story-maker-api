namespace StoryMakerApi.Dtos.Chapter;

public sealed record CreateChapterRequest(string Title, string Content, int? SequenceNumber = null);
