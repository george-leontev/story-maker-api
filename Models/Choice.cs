namespace StoryMakerApi.Models;

public sealed class Choice
{
    public Guid Id { get; init; }
    public Guid ChapterId { get; init; }
    public required string Option1Text { get; init; }
    public required string Option2Text { get; init; }
    public int Option1Votes { get; set; }
    public int Option2Votes { get; set; }
    public DateTime ExpiresAt { get; init; }
    public bool IsClosed { get; set; }

    public Chapter Chapter { get; init; } = null!;
    public ICollection<Vote> Votes { get; init; } = new List<Vote>();
}
