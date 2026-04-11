namespace StoryMakerApi.Models;

public sealed class Chapter
{
    public Guid Id { get; init; }
    public Guid StoryId { get; init; }
    public required string Content { get; init; }
    public int SequenceNumber { get; init; }
    public DateTime CreatedAt { get; init; }

    public Story Story { get; init; } = null!;
    public Choice? Choice { get; init; }
}
