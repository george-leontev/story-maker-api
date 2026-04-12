namespace StoryMakerApi.Dtos.Comment;

public sealed record CommentResponse(int Id, int StoryId, string UserUsername, string Text, DateTime Timestamp);
