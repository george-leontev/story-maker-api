namespace StoryMakerApi.Models;

public sealed class Chapter
{
    public int Id { get; init; }
    public int StoryId { get; init; }
    public required string Content { get; init; }
    public int SequenceNumber { get; init; }
    public DateTime CreatedAt { get; init; }

    public Story Story { get; init; } = null!;
    public Choice? Choice { get; init; }
}
