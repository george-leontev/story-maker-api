namespace StoryMakerApi.Models;

public sealed class Chapter
{
    public int Id { get; init; }
    public int StoryId { get; init; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
    public DateTime CreatedAt { get; init; }

    public Story Story { get; init; } = null!;
    public Choice? Choice { get; init; }
}
