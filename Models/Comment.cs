namespace StoryMakerApi.Models;

public sealed class Comment
{
    public Guid Id { get; init; }
    public Guid StoryId { get; init; }
    public Guid UserId { get; init; }
    public required string Text { get; init; }
    public DateTime Timestamp { get; init; }

    public Story Story { get; init; } = null!;
    public User User { get; init; } = null!;
}
