namespace StoryMakerApi.Models;

public sealed class Comment
{
    public int Id { get; init; }
    public int StoryId { get; init; }
    public int UserId { get; init; }
    public required string Text { get; init; }
    public DateTime Timestamp { get; init; }

    public Story Story { get; init; } = null!;
    public User User { get; init; } = null!;
}
